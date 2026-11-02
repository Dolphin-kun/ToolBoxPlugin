using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Shortcuts;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Resources.Icons;

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

        private IReadOnlyList<object> groups = [];
        public IReadOnlyList<object> Groups
        {
            get => groups;
            private set => Set(ref groups, value);
        }

        private object? selectedDetail;
        public object? SelectedDetail
        {
            get => selectedDetail;
            set => Set(ref selectedDetail, value);
        }

        private IReadOnlyList<SettingsSearchResult> searchIndex = [];

        private string searchText = string.Empty;
        public string SearchText
        {
            get => searchText;
            set
            {
                if (Set(ref searchText, value ?? string.Empty, etcChangedPropertyNames: nameof(IsSearching)))
                    UpdateSearchResults();
            }
        }

        public bool IsSearching => !string.IsNullOrWhiteSpace(SearchText);

        private IReadOnlyList<SettingsSearchResult> searchResults = [];
        public IReadOnlyList<SettingsSearchResult> SearchResults
        {
            get => searchResults;
            private set => Set(ref searchResults, value, etcChangedPropertyNames: nameof(HasNoSearchResults));
        }

        public bool HasNoSearchResults => IsSearching && SearchResults.Count == 0;

        private void UpdateSearchResults()
        {
            var terms = SearchText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            SearchResults = terms.Length == 0 ? [] : [.. searchIndex.Where(result => result.Matches(terms))];
        }

        public ToolBoxSettingsViewModel(ToolBoxSettings settings, ToolBoxPanel panel)
        {
            this.panel = panel;
            basicSettings = new BasicSettingsViewModel(settings);
            generalSettings = new GeneralSettingsViewModel(settings, panel);
            launcherSettings = new LauncherSettingsViewModel(settings, panel);
            itemStockSettings = new ItemStockSettingsViewModel();
            addonSettings = new AddonSettingsViewModel();
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
            UpdateSearchResults();

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
        }

        private IReadOnlyList<object> BuildGroups()
        {
            var draggableItems = ItemCatalog.GetDraggableItemsBySettingsType();
            var allSettingsTypes = ItemCatalog.GetSettingsPrototypes();

            var categoryOrder = new Dictionary<ItemCategory, (int Order, string JaName)>
            {
                { ItemCategory.Timeline,   (0, Texts.CategoryTimeline) },
                { ItemCategory.Layout,     (1, Texts.CategoryLayout) },
                { ItemCategory.Animation,  (2, Texts.CategoryAnimation) },
                { ItemCategory.Text,       (3, Texts.CategoryText) },
                { ItemCategory.Audio,      (4, Texts.CategoryAudio) },
                { ItemCategory.Utility,    (5, Texts.CategoryUtility) },
                { ItemCategory.Decoration, (6, Texts.CategoryDecoration) },
                { ItemCategory.None,       (99, Texts.CategoryOther) },
            };

            var settingsRoot = new RootCategoryGroupViewModel(Texts.SettingsRootTitle, [
                basicSettings,
                generalSettings,
                launcherSettings,
                itemStockSettings,
                addonSettings,
            ]);

            var index = new List<SettingsSearchResult>();
            foreach (var node in settingsRoot.SubGroups.OfType<SettingsNodeViewModel>())
                index.Add(new SettingsSearchResult(node, node.CategoryName, IconKeys.Cog, Texts.SettingsRootTitle, []));

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
                            var entry = new ToolItemEntryViewModel(panel, s.GetType(), s.DisplayName, s.IconPath, s.Description, item?.SettingsViewType);

                            var commandNames = item?.GetCommandDefinitions().Select(definition => definition.DisplayName) ?? [];
                            index.Add(new SettingsSearchResult(entry, entry.DisplayName, entry.IconPath, jaName, [entry.Description, .. commandNames]));
                            return entry;
                        })
                        .ToList();
                    return new ToolItemGroupViewModel(jaName, entries);
                })
                .ToList();

            itemGroups.AddRange(toolGroups);

            var shortcutGroups = ShortcutDefinitions.Groups
                .Select(group => (object)new ShortcutsGroupViewModel(panel, group))
                .ToList();

            itemGroups.Add(new RootCategoryGroupViewModel(Texts.ShortcutsRootTitle, shortcutGroups) { IsExpanded = false });

            foreach (var group in shortcutGroups.OfType<ShortcutsGroupViewModel>())
            {
                foreach (var shortcut in group.Shortcuts)
                {
                    index.Add(new SettingsSearchResult(
                        shortcut, shortcut.DisplayName, shortcut.IconPath,
                        $"{Texts.ShortcutsRootTitle} / {group.CategoryName}", [shortcut.Description]));
                }
            }

            searchIndex = index;

            var itemsRoot = new RootCategoryGroupViewModel(Texts.ItemsRootTitle, itemGroups);

            return [settingsRoot, itemsRoot];
        }

        private ICommand? selectDetailCommand;
        public ICommand SelectDetailCommand =>
            selectDetailCommand ??= new ActionCommand(_ => true, param => SelectedDetail = param);
    }
}
