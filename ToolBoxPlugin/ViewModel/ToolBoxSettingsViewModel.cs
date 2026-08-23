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
                .FirstOrDefault(g => g.CategoryName == Texts.SettingsRootTitle)?
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
                { ItemCategory.Timeline,   (0, Texts.CategoryTimeline) },
                { ItemCategory.Layout,     (1, Texts.CategoryLayout) },
                { ItemCategory.Text,       (2, Texts.CategoryText) },
                { ItemCategory.Audio,      (3, Texts.CategoryAudio) },
                { ItemCategory.Utility,    (4, Texts.CategoryUtility) },
                { ItemCategory.Decoration, (5, Texts.CategoryDecoration) },
                { ItemCategory.None,       (99, Texts.CategoryOther) },
            };

            var settingsRoot = new RootCategoryGroupViewModel(Texts.SettingsRootTitle, [
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

            itemGroups.Add(new RootCategoryGroupViewModel(Texts.ShortcutsRootTitle, shortcutGroups) { IsExpanded = false });

            var itemsRoot = new RootCategoryGroupViewModel(Texts.ItemsRootTitle, itemGroups);

            return [settingsRoot, itemsRoot];
        }

        private ICommand? selectDetailCommand;
        public ICommand SelectDetailCommand =>
            selectDetailCommand ??= new ActionCommand(_ => true, param => SelectedDetail = param);
    }
}
