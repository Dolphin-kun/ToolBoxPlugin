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

        public AddonSettingsViewModel() : base("アドオン")
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
                    $"フォルダーを開けませんでした。\n\n{directory}\n\n{ex.Message}",
                    "アドオン",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private static void DeleteAddon(AddonEntryViewModel addon)
        {
            var confirm = MessageBox.Show(
                $"アドオン「{addon.Name}」を削除します。\n\n{addon.SourcePath}",
                "アドオンの削除",
                MessageBoxButton.OKCancel);

            if (confirm != MessageBoxResult.OK)
                return;

            try
            {
                if (!AddonLoader.DeleteAddonFile(addon.SourcePath))
                {
                    MessageBox.Show(
                        $"アドオン「{addon.Name}」を無効化しました。\n" +
                        "読み込み済みのため、ファイルの削除と機能の解放は YMM4 の再起動時に完了します。",
                        "アドオンの削除",
                        MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"アドオン「{addon.Name}」を削除できませんでした。\n\n{ex.Message}",
                    "アドオンの削除",
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
            Description = "無効にしています。有効に戻すと次の読み込みで反映されます。",
            isEnabled = false,
        };
    }
}
