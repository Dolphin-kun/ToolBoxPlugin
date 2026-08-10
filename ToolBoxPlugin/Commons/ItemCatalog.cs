using ToolBox.Items;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    /// <summary>本体組み込みのアイテムとアドオンのアイテムをまとめて列挙する。</summary>
    public static class ItemCatalog
    {
        public static IReadOnlyList<DraggableItem> GetDraggableItems()
        {
            var items = new List<DraggableItem>(GetBuiltInDraggableItems());
            items.AddRange(AddonLoader.GetAllDraggableItems());
            return items;
        }

        public static Dictionary<Type, DraggableItem> GetDraggableItemsBySettingsType()
        {
            var map = new Dictionary<Type, DraggableItem>();
            foreach (var item in GetDraggableItems())
                map.TryAdd(item.SettingsType, item);
            return map;
        }

        public static IReadOnlyList<ToolItemSettings> GetSettingsPrototypes()
        {
            var prototypes = new List<ToolItemSettings>(GetBuiltInSettingsPrototypes());
            prototypes.AddRange(AddonLoader.GetAllSettings());
            return prototypes;
        }

        public static HashSet<Type> GetKnownSettingsTypes()
        {
            var types = new HashSet<Type>(
                typeof(DraggableItem).Assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(ToolItemSettings).IsAssignableFrom(t)));

            foreach (var settings in AddonLoader.GetAllSettings())
                types.Add(settings.GetType());

            return types;
        }

        private static IEnumerable<DraggableItem> GetBuiltInDraggableItems() =>
            typeof(DraggableItem).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(DraggableItem).IsAssignableFrom(t))
                .Select(t => (DraggableItem)Activator.CreateInstance(t)!);

        private static IEnumerable<ToolItemSettings> GetBuiltInSettingsPrototypes() =>
            typeof(ToolItemSettings).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                    && typeof(ToolItemSettings).IsAssignableFrom(t)
                    && t != typeof(ToolItemSettings)
                    && t != typeof(Items.Shortcuts.Settings.ShortcutSettings))
                .Select(t => (ToolItemSettings)Activator.CreateInstance(t)!);
    }
}
