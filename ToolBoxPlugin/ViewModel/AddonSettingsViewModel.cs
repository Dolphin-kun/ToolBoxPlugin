using System.Windows;
using System.Windows.Input;
using ToolBox.Commons;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class AddonSettingsViewModel : Bindable
    {
        private IReadOnlyList<AddonLoader.LoadedAddon> loadedAddons = [];

        public string CategoryName { get; } = "アドオン";

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }

        public IReadOnlyList<AddonLoader.LoadedAddon> LoadedAddons
        {
            get => loadedAddons;
            private set => Set(ref loadedAddons, value);
        }

        public ICommand DeleteAddonCommand { get; }

        public AddonSettingsViewModel()
        {
            DeleteAddonCommand = new ActionCommand(
                param => param is AddonLoader.LoadedAddon,
                param =>
                {
                    if (param is AddonLoader.LoadedAddon addon)
                        DeleteAddon(addon);
                });

            Refresh();
            AddonLoader.AddonsChanged += OnAddonsChanged;
        }

        private static void DeleteAddon(AddonLoader.LoadedAddon addon)
        {
            var confirm = MessageBox.Show(
                $"アドオン「{addon.Name}」を削除します。\n\n{addon.SourcePath}",
                "アドオンの削除",
                MessageBoxButton.OKCancel);

            if (confirm != MessageBoxResult.OK)
                return;

            try
            {
                if (!AddonLoader.DeleteAddon(addon))
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
            LoadedAddons = AddonLoader.GetLoadedAddons();
        }

        private void OnAddonsChanged(object? sender, EventArgs e)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(Refresh);
                return;
            }

            Refresh();
        }
    }
}
