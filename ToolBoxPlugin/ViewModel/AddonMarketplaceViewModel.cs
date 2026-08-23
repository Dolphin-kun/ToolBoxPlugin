using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ToolBox.Commons;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class AddonMarketplaceViewModel : SettingsNodeViewModel
    {
        public ObservableCollection<MarketplaceAddonViewModel> Addons { get; } = [];

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            private set => Set(ref isBusy, value, etcChangedPropertyNames: nameof(IsIdle));
        }

        public bool IsIdle => !IsBusy;

        private string statusMessage = Texts.AddonMarketplaceInitialStatus;
        public string StatusMessage
        {
            get => statusMessage;
            private set => Set(ref statusMessage, value);
        }

        public bool HasAddons => Addons.Count > 0;

        public ICommand RefreshCommand { get; }
        public ICommand InstallCommand { get; }
        public ICommand OpenRepositoryCommand { get; }

        public AddonMarketplaceViewModel() : base(Texts.AddonMarketplaceTitle)
        {
            RefreshCommand = new ActionCommand(_ => !IsBusy, _ => _ = RefreshAsync());
            InstallCommand = new ActionCommand(
                param => !IsBusy && param is MarketplaceAddonViewModel,
                param =>
                {
                    if (param is MarketplaceAddonViewModel addon)
                        _ = InstallAsync(addon);
                });
            OpenRepositoryCommand = new ActionCommand(
                param => param is MarketplaceAddonViewModel,
                param =>
                {
                    if (param is MarketplaceAddonViewModel addon)
                        OpenUrl(addon.Source.HtmlUrl);
                });

            AddonLoader.AddonsChanged += OnAddonsChanged;
        }

        private async Task RefreshAsync()
        {
            IsBusy = true;
            StatusMessage = Texts.AddonMarketplaceLoading;

            try
            {
                var response = await AddonMarketplace.SearchAsync();

                Addons.Clear();
                foreach (var addon in response.Addons)
                    Addons.Add(new MarketplaceAddonViewModel(addon));

                StatusMessage = BuildStatusMessage(response);
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(Texts.AddonMarketplaceLoadFailed, ex.Message);
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(HasAddons));
                RaiseCommandStates();
            }
        }

        private static string BuildStatusMessage(SearchResponse response)
        {
            if (response.Addons.Count == 0)
                return string.Format(Texts.AddonMarketplaceNoResults, AddonMarketplace.Topic);

            var message = string.Format(Texts.AddonMarketplaceFound, response.Addons.Count);

            if (response.IsTruncated)
                message += " " + string.Format(Texts.AddonMarketplaceTruncated, response.TotalCount);

            message += response.IsFromCache
                ? " " + string.Format(Texts.AddonMarketplaceCached, response.CacheExpiresAt.LocalDateTime.ToString("HH:mm"))
                : string.Empty;

            return message;
        }

        private async Task InstallAsync(MarketplaceAddonViewModel addon)
        {
            IsBusy = true;
            StatusMessage = string.Format(Texts.AddonMarketplaceInstalling, addon.Source.Name);

            try
            {
                var installed = await AddonMarketplace.InstallAsync(addon.Source);
                StatusMessage = string.Format(Texts.AddonMarketplaceInstallSucceeded, addon.Source.Name, string.Join(", ", installed));
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(Texts.AddonMarketplaceInstallFailedStatus, addon.Source.Name, ex.Message);
                MessageBox.Show(
                    $"{Texts.AddonMarketplaceInstallFailed}\n\n{ex.Message}",
                    Texts.AddonMarketplaceTitle,
                    MessageBoxButton.OK);
            }
            finally
            {
                IsBusy = false;
                RefreshInstalledFlags();
                RaiseCommandStates();
            }
        }

        private static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to open {url}: {ex}");
            }
        }

        private void OnAddonsChanged(object? sender, EventArgs e)
        {
            UiThread.Run(RefreshInstalledFlags);
        }

        private void RefreshInstalledFlags()
        {
            var installedUrls = AddonLoader.GetLoadedAddons()
                .Select(a => a.RepositoryUrl)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url!.TrimEnd('/'))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var addon in Addons)
                addon.IsInstalled = installedUrls.Contains(addon.Source.HtmlUrl.TrimEnd('/'));
        }

        private void RaiseCommandStates()
        {
            (RefreshCommand as ActionCommand)?.RaiseCanExecuteChanged();
            (InstallCommand as ActionCommand)?.RaiseCanExecuteChanged();
        }
    }

    public class MarketplaceAddonViewModel(MarketplaceAddon source) : Bindable
    {
        public MarketplaceAddon Source { get; } = source;

        public string Name => Source.Name;
        public string Owner => Source.Owner;
        public string Description => Source.Description;
        public string StarsText => Source.StarsText;
        public string UpdatedText => Source.UpdatedText;
        public string HtmlUrl => Source.HtmlUrl;

        private bool isInstalled;
        public bool IsInstalled
        {
            get => isInstalled;
            set => Set(ref isInstalled, value);
        }
    }
}
