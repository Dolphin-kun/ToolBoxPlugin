using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Commons
{
    public static class AddonLoader
    {
        private const string PluginFolderName = "ToolBox";
        private const string AddonSubFolderName = "Addons";
        private const string PendingDeleteExtension = ".deleted";
        private static readonly Lock lockObject = new();
        private static readonly List<LoadedAddon> loadedAddons = [];
        private static FileSystemWatcher? watcher;
        private static Timer? refreshTimer;

        public static event EventHandler? AddonsChanged;

        public static string AddonDirectory => Path.Combine(
            AppDirectories.PluginDirectory,
            PluginFolderName,
            AddonSubFolderName);

        public static IReadOnlyList<LoadedAddon> GetLoadedAddons()
        {
            lock (lockObject)
            {
                return [.. loadedAddons];
            }
        }

        public static IReadOnlyList<DraggableItem> GetAllDraggableItems()
        {
            lock (lockObject)
            {
                var items = new List<DraggableItem>();
                foreach (var addon in loadedAddons)
                {
                    items.AddRange(addon.DraggableItems);
                }

                return items;
            }
        }

        public static IReadOnlyList<ToolItemSettings> GetAllSettings()
        {
            lock (lockObject)
            {
                var settings = new List<ToolItemSettings>();
                foreach (var addon in loadedAddons)
                {
                    settings.AddRange(addon.Settings);
                }

                return settings;
            }
        }

        public static void LoadAddons(string? baseDirectory = null)
        {
            lock (lockObject)
            {
                var searchDirectories = GetSearchDirectories(baseDirectory).ToList();

                foreach (var directory in searchDirectories)
                {
                    Directory.CreateDirectory(directory);
                    PurgePendingDeletions(directory);
                }

                var files = searchDirectories
                    .Where(Directory.Exists)
                    .SelectMany(directory => Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var currentPaths = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);

                for (var index = loadedAddons.Count - 1; index >= 0; index--)
                {
                    var addon = loadedAddons[index];
                    if (!currentPaths.Contains(addon.SourcePath))
                    {
                        loadedAddons.RemoveAt(index);
                        UnloadAddon(addon);
                    }
                }

                foreach (var file in files)
                {
                    if (ShouldSkipAssembly(file))
                        continue;

                    DateTime lastWriteTimeUtc;
                    try
                    {
                        lastWriteTimeUtc = File.GetLastWriteTimeUtc(file);
                    }
                    catch (IOException)
                    {
                        continue;
                    }

                    var existingIndex = loadedAddons.FindIndex(a => string.Equals(a.SourcePath, file, StringComparison.OrdinalIgnoreCase));
                    if (existingIndex >= 0)
                    {
                        if (loadedAddons[existingIndex].LastWriteTimeUtc == lastWriteTimeUtc)
                            continue;

                        var previous = loadedAddons[existingIndex];
                        loadedAddons.RemoveAt(existingIndex);
                        UnloadAddon(previous);
                    }

                    TryLoadAddon(file, lastWriteTimeUtc);
                }

                EnsureWatcherStarted();
            }

            AddonsChanged?.Invoke(null, EventArgs.Empty);
        }

        public static bool DeleteAddon(LoadedAddon addon)
        {
            bool deleted;
            try
            {
                File.Delete(addon.SourcePath);
                deleted = true;
            }
            catch (Exception)
            {
                File.Move(addon.SourcePath, GetPendingDeletePath(addon.SourcePath), overwrite: true);
                deleted = false;
            }

            LoadAddons();
            return deleted;
        }

        private static string GetPendingDeletePath(string assemblyPath)
        {
            var candidate = assemblyPath + PendingDeleteExtension;
            var index = 1;
            while (File.Exists(candidate))
            {
                candidate = $"{assemblyPath}.{index++}{PendingDeleteExtension}";
            }

            return candidate;
        }

        private static void PurgePendingDeletions(string directory)
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*" + PendingDeleteExtension, SearchOption.TopDirectoryOnly))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to purge pending deletion {file}: {ex}");
                }
            }
        }

        private static void UnloadAddon(LoadedAddon addon)
        {
            try
            {
                addon.Context.Unload();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to unload addon {addon.SourcePath}: {ex}");
            }
        }

        private static void EnsureWatcherStarted()
        {
            if (watcher != null)
                return;

            Directory.CreateDirectory(AddonDirectory);

            watcher = new FileSystemWatcher(AddonDirectory, "*.dll")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true
            };
            watcher.Created += OnAddonDirectoryChanged;
            watcher.Changed += OnAddonDirectoryChanged;
            watcher.Deleted += OnAddonDirectoryChanged;
            watcher.Renamed += OnAddonDirectoryRenamed;
        }

        private static void OnAddonDirectoryChanged(object sender, FileSystemEventArgs e) => ScheduleRefresh();

        private static void OnAddonDirectoryRenamed(object sender, RenamedEventArgs e) => ScheduleRefresh();

        private static void ScheduleRefresh()
        {
            lock (lockObject)
            {
                refreshTimer?.Dispose();
                refreshTimer = new Timer(_ => LoadAddons(), null, 300, Timeout.Infinite);
            }
        }

        private static bool ShouldSkipAssembly(string assemblyPath)
        {
            var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
            return assemblyName.Equals("ToolBox", StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> GetSearchDirectories(string? baseDirectory)
        {
            if (!string.IsNullOrWhiteSpace(baseDirectory))
            {
                yield return baseDirectory;
                yield break;
            }

            yield return AddonDirectory;
        }

        private static void TryLoadAddon(string assemblyPath, DateTime lastWriteTimeUtc)
        {
            var contextName = $"{Path.GetFileNameWithoutExtension(assemblyPath)}_{Guid.NewGuid():N}";
            var context = new AssemblyLoadContext(contextName, isCollectible: true);

            context.Resolving += (alc, name) =>
            {
                return AssemblyLoadContext.Default.Assemblies
                    .FirstOrDefault(a => string.Equals(a.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase));
            };

            try
            {
                AssemblyName.GetAssemblyName(assemblyPath);

                var assembly = context.LoadFromAssemblyPath(assemblyPath);

                var addonFound = false;
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(IToolBoxAddon).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract)
                        continue;

                    var addon = (IToolBoxAddon?)Activator.CreateInstance(type);
                    if (addon == null)
                        continue;

                    loadedAddons.Add(new LoadedAddon(assemblyPath, addon, context, lastWriteTimeUtc));
                    addonFound = true;
                }

                if (!addonFound)
                {
                    context.Unload();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load addon {assemblyPath}: {ex}");
                try { context.Unload(); } catch { /* ignore */ }
            }
        }

        public sealed class LoadedAddon(string sourcePath, IToolBoxAddon addon, AssemblyLoadContext context, DateTime lastWriteTimeUtc)
        {
            public string SourcePath { get; } = sourcePath;
            public IToolBoxAddon Addon { get; } = addon;
            internal AssemblyLoadContext Context { get; } = context;
            internal DateTime LastWriteTimeUtc { get; } = lastWriteTimeUtc;
            public string Name { get; } = addon.Name;
            public string Description { get; } = addon.Description;
            public string Author { get; } = addon.Author;
            public string Version { get; } = addon.Version;
            public string? RepositoryUrl { get; } = addon.RepositoryUrl;
            public IReadOnlyList<DraggableItem> DraggableItems { get; } = [.. addon.GetDraggableItems()];
            public IReadOnlyList<ToolItemSettings> Settings { get; } = [.. addon.GetSettings()];
        }
    }
}
