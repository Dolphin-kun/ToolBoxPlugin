using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ToolBox.Commons;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class AddonSettingsViewModel : SettingsNodeViewModel
    {
        public ObservableCollection<AddonEntryViewModel> Addons { get; } = [];

        public bool HasAddons => Addons.Count > 0;

        public ICommand DeleteAddonCommand { get; }
        public ICommand OpenAddonFolderCommand { get; }
        public ICommand ReloadCommand { get; }

        public AddonSettingsViewModel() : base(Texts.AddonSettingsTitle)
        {
            DeleteAddonCommand = new ActionCommand(
                param => param is AddonEntryViewModel,
                param =>
                {
                    if (param is AddonEntryViewModel addon)
                        DeleteAddon(addon);
                });

            OpenAddonFolderCommand = new ActionCommand(_ => true, _ => OpenAddonFolder());
            ReloadCommand = new ActionCommand(_ => true, _ => AddonLoader.LoadAddons());

            Refresh();
            AddonLoader.AddonsChanged += OnAddonsChanged;
        }

        private static void OpenAddonFolder()
        {
            var directory = AddonLoader.AddonDirectory;
            Directory.CreateDirectory(directory);

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(directory)
                {
                    UseShellExecute = true,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{Texts.AddonOpenFolderFailed}\n\n{directory}\n\n{ex.Message}",
                    Texts.AddonSettingsTitle,
                    MessageBoxButton.OK);
            }
        }

        private static void DeleteAddon(AddonEntryViewModel addon)
        {
            var confirm = MessageBox.Show(
                $"{string.Format(Texts.AddonDeleteConfirm, addon.Name)}\n\n{addon.SourcePath}",
                Texts.AddonDeleteCaption,
                MessageBoxButton.OKCancel);

            if (confirm != MessageBoxResult.OK)
                return;

            try
            {
                if (!AddonLoader.DeleteAddonFile(addon.SourcePath))
                {
                    MessageBox.Show(
                        $"{string.Format(Texts.AddonDisabled, addon.Name)}\n" +
                        Texts.AddonDisabledHint,
                        Texts.AddonDeleteCaption,
                        MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{string.Format(Texts.AddonDeleteFailed, addon.Name)}\n\n{ex.Message}",
                    Texts.AddonDeleteCaption,
                    MessageBoxButton.OK);
            }
        }

        public void Refresh()
        {
            Addons.Clear();

            foreach (var addon in AddonLoader.GetLoadedAddons())
                Addons.Add(AddonEntryViewModel.FromLoaded(addon));

            foreach (var path in AddonLoader.GetDisabledAddonFiles())
                Addons.Add(AddonEntryViewModel.FromDisabled(path));

            OnPropertyChanged(nameof(HasAddons));
        }

        private void OnAddonsChanged(object? sender, EventArgs e)
        {
            UiThread.Run(Refresh);
        }
    }

    public class AddonEntryViewModel : Bindable
    {
        public required string Name { get; init; }
        public required string SourcePath { get; init; }
        public string Version { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? RepositoryUrl { get; init; }

        public bool HasRepository => !string.IsNullOrWhiteSpace(RepositoryUrl);

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value)
                    return;

                isEnabled = value;
                OnPropertyChanged();

                UiThread.Post(() => AddonLoader.SetAddonEnabled(SourcePath, value));
            }
        }

        public static AddonEntryViewModel FromLoaded(AddonLoader.LoadedAddon addon) => new()
        {
            Name = addon.Name,
            SourcePath = addon.SourcePath,
            Version = addon.Version,
            Author = addon.Author,
            Description = addon.Description,
            RepositoryUrl = addon.RepositoryUrl,
            isEnabled = true,
        };

        public static AddonEntryViewModel FromDisabled(string path) => new()
        {
            Name = Path.GetFileNameWithoutExtension(path),
            SourcePath = path,
            Description = Texts.AddonDisabledDescription,
            isEnabled = false,
        };
    }
}
