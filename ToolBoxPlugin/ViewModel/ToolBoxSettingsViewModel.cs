using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Shortcuts;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ToolBoxSettingsViewModel : Bindable
    {
        private readonly ToolBoxPanel panel;
        private readonly BasicSettingsViewModel basicSettings;
        private readonly GeneralSettingsViewModel generalSettings;
        private readonly LauncherSettingsViewModel launcherSettings;
        private readonly ItemStockSettingsViewModel itemStockSettings;
        private readonly AddonSettingsViewModel addonSettings;
        private readonly AddonMarketplaceViewModel addonMarketplace;

        private IReadOnlyList<object> groups = [];
        public IReadOnlyList<object> Groups
        {
            get => groups;
            private set => Set(ref groups, value);
        }

        public ICommand CloseCommand { get; }

        private object? selectedDetail;
        public object? SelectedDetail
        {
            get => selectedDetail;
            set => Set(ref selectedDetail, value);
        }

        public ToolBoxSettingsViewModel(ToolBoxSettings settings, ToolBoxPanel panel, Action closeAction)
        {
            this.panel = panel;
            basicSettings = new BasicSettingsViewModel(settings);
            generalSettings = new GeneralSettingsViewModel(settings, panel);
            launcherSettings = new LauncherSettingsViewModel(settings, panel);
            itemStockSettings = new ItemStockSettingsViewModel();
            addonSettings = new AddonSettingsViewModel();
            addonMarketplace = new AddonMarketplaceViewModel();
            CloseCommand = new ActionCommand(_ => true, _ => closeAction());
            Groups = BuildGroups();
            AddonLoader.AddonsChanged += OnAddonsChanged;

            var initialBasicSettings = Groups.OfType<RootCategoryGroupViewModel>()
                .FirstOrDefault(g => g.CategoryName == "設定")?
                .SubGroups.OfType<BasicSettingsViewModel>()
                .FirstOrDefault();
            if (initialBasicSettings != null)
            {
                SelectedDetail = initialBasicSettings;
            }
        }

        private void OnAddonsChanged(object? sender, EventArgs e)
        {
            UiThread.Run(ReloadGroups);
        }

        private void ReloadGroups()
        {
            var currentSelection = SelectedDetail;
            Groups = BuildGroups();

            if (currentSelection is BasicSettingsViewModel)
            {
                SelectedDetail = basicSettings;
            }
            else if (currentSelection is GeneralSettingsViewModel)
            {
                SelectedDetail = generalSettings;
            }
            else if (currentSelection is LauncherSettingsViewModel)
            {
                SelectedDetail = launcherSettings;
            }
            else if (currentSelection is ItemStockSettingsViewModel)
            {
                SelectedDetail = itemStockSettings;
            }
            else if (currentSelection is AddonSettingsViewModel)
            {
                SelectedDetail = addonSettings;
            }
            else if (currentSelection is AddonMarketplaceViewModel)
            {
                SelectedDetail = addonMarketplace;
            }
        }

        private IReadOnlyList<object> BuildGroups()
        {
            var draggableItems = ItemCatalog.GetDraggableItemsBySettingsType();
            var allSettingsTypes = ItemCatalog.GetSettingsPrototypes();

            var categoryOrder = new Dictionary<ItemCategory, (int Order, string JaName)>
            {
                { ItemCategory.Timeline,   (0, "タイムライン") },
                { ItemCategory.Layout,     (1, "レイアウト") },
                { ItemCategory.Text,       (2, "テキスト") },
                { ItemCategory.Audio,      (3, "オーディオ") },
                { ItemCategory.Utility,    (4, "ユーティリティ") },
                { ItemCategory.Decoration, (5, "装飾") },
                { ItemCategory.None,       (99, "その他") },
            };

            var settingsRoot = new RootCategoryGroupViewModel("設定", [
                basicSettings,
                generalSettings,
                launcherSettings,
                itemStockSettings,
                addonSettings,
                addonMarketplace
            ]);

            var itemGroups = new List<object>();

            var toolGroups = allSettingsTypes
                .GroupBy(s => s.Category)
                .OrderBy(g => categoryOrder.TryGetValue(g.Key, out var info) ? info.Order : 50)
                .Select(g =>
                {
                    var jaName = categoryOrder.TryGetValue(g.Key, out var info) ? info.JaName : g.Key.ToString();
                    var entries = g
                        .Select(s =>
                        {
                            draggableItems.TryGetValue(s.GetType(), out var item);
                            return new ToolItemEntryViewModel(panel, s.GetType(), s.DisplayName, s.IconPath, s.Description, item?.SettingsViewType);
                        })
                        .ToList();
                    return new ToolItemGroupViewModel(jaName, entries);
                });

            itemGroups.AddRange(toolGroups);

            var shortcutGroups = ShortcutDefinitions.Groups
                .Select(group => (object)new ShortcutsGroupViewModel(panel, group))
                .ToList();

            itemGroups.Add(new RootCategoryGroupViewModel("ショートカット", shortcutGroups) { IsExpanded = false });

            var itemsRoot = new RootCategoryGroupViewModel("アイテム", itemGroups);

            return [settingsRoot, itemsRoot];
        }

        private ICommand? selectDetailCommand;
        public ICommand SelectDetailCommand =>
            selectDetailCommand ??= new ActionCommand(_ => true, param => SelectedDetail = param);
    }
}
