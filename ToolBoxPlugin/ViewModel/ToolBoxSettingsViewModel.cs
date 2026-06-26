using System.Windows.Input;
using ToolBox.Items;
using ToolBox.Items.Shortcuts.Settings;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ToolBoxSettingsViewModel : Bindable
    {
        private readonly ToolBoxSettings settings;

        public IReadOnlyList<object> Groups { get; }

        public ICommand CloseCommand { get; }

        public static string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        private object? selectedDetail;
        public object? SelectedDetail
        {
            get => selectedDetail;
            set => Set(ref selectedDetail, value);
        }

        public ToolItemEntryViewModel? SelectedToolEntry => SelectedDetail as ToolItemEntryViewModel;
        public ShortcutEntryViewModel? SelectedShortcutEntry => SelectedDetail as ShortcutEntryViewModel;
        public ShortcutsGroupViewModel? SelectedShortcutsGroup => SelectedDetail as ShortcutsGroupViewModel;
        public GeneralSettingsViewModel? SelectedGeneralSettings => SelectedDetail as GeneralSettingsViewModel;
        public BasicSettingsViewModel? SelectedBasicSettings => SelectedDetail as BasicSettingsViewModel;

        public ToolBoxSettingsViewModel(ToolBoxSettings settings, Action closeAction)
        {
            this.settings = settings;
            CloseCommand = new ActionCommand(_ => true, _ => closeAction());
            Groups = BuildGroups();

            var basicSettings = Groups.OfType<RootCategoryGroupViewModel>()
                .FirstOrDefault(g => g.CategoryName == "設定")?
                .SubGroups.OfType<BasicSettingsViewModel>()
                .FirstOrDefault();
            if (basicSettings != null)
            {
                SelectedDetail = basicSettings;
            }
        }

        private IReadOnlyList<object> BuildGroups()
        {
            var draggableItems = typeof(DraggableItem).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(DraggableItem).IsAssignableFrom(t))
                .Select(t => (DraggableItem)Activator.CreateInstance(t)!)
                .ToDictionary(item => item.SettingsType, item => item);

            var allSettingsTypes = typeof(ToolItemSettings).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                    && typeof(ToolItemSettings).IsAssignableFrom(t)
                    && t != typeof(ToolItemSettings)
                    && t != typeof(ShortcutSettings))
                .Select(t => (ToolItemSettings)Activator.CreateInstance(t)!)
                .ToList();

            var categoryOrder = new Dictionary<ItemCategory, (int Order, string JaName)>
            {
                { ItemCategory.Timeline, (0, "タイムライン") },
                { ItemCategory.Layout,   (1, "レイアウト") },
                { ItemCategory.Text,     (2, "テキスト") },
                { ItemCategory.Audio,    (3, "オーディオ") },
                { ItemCategory.None,     (99, "その他") },
            };

            var settingsRoot = new RootCategoryGroupViewModel("設定", [
                new BasicSettingsViewModel(settings),
                new GeneralSettingsViewModel(settings)
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
                            return new ToolItemEntryViewModel(settings, s.GetType(), s.DisplayName, s.IconPath, item?.SettingsViewType);
                        })
                        .ToList();
                    return new ToolItemGroupViewModel(jaName, entries);
                });

            itemGroups.AddRange(toolGroups);

            itemGroups.Add(new ShortcutsGroupViewModel(settings));

            var itemsRoot = new RootCategoryGroupViewModel("アイテム", itemGroups);

            return [settingsRoot, itemsRoot];
        }

        private ICommand? selectToolEntryCommand;
        public ICommand SelectToolEntryCommand =>
            selectToolEntryCommand ??= new ActionCommand(_ => true, param =>
            {
                if (param is ToolItemEntryViewModel entry)
                    SelectedDetail = entry;
            });

        private ICommand? selectShortcutEntryCommand;
        public ICommand SelectShortcutEntryCommand =>
            selectShortcutEntryCommand ??= new ActionCommand(_ => true, param =>
            {
                if (param is ShortcutEntryViewModel entry)
                    SelectedDetail = entry;
            });

        private ICommand? selectShortcutsGroupCommand;
        public ICommand SelectShortcutsGroupCommand =>
            selectShortcutsGroupCommand ??= new ActionCommand(_ => true, param =>
            {
                if (param is ShortcutsGroupViewModel group)
                    SelectedDetail = group;
            });

        private ICommand? selectGeneralSettingsCommand;
        public ICommand SelectGeneralSettingsCommand =>
            selectGeneralSettingsCommand ??= new ActionCommand(_ => true, param =>
            {
                if (param is GeneralSettingsViewModel group)
                    SelectedDetail = group;
            });

        private ICommand? selectBasicSettingsCommand;
        public ICommand SelectBasicSettingsCommand =>
            selectBasicSettingsCommand ??= new ActionCommand(_ => true, param =>
            {
                if (param is BasicSettingsViewModel group)
                    SelectedDetail = group;
            });
    }
}
