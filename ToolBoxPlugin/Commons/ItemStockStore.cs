using System.Collections.ObjectModel;
using ToolBox.Settings;
using YukkuriMovieMaker.Project.Items;
using YmmJson = YukkuriMovieMaker.Json.Json;

namespace ToolBox.Commons
{
    public static class ItemStockStore
    {
        public static ObservableCollection<ItemStockEntry> Entries => ToolBoxSettings.Default.ItemStocks;

        public static ItemStockEntry? Find(Guid id) =>
            id == Guid.Empty ? null : Entries.FirstOrDefault(entry => entry.Id == id);

        public static ItemStockEntry? Create(IReadOnlyList<IItem> items, string? name = null)
        {
            if (items.Count == 0)
                return null;

            try
            {
                var clones = items.Select(item => item.GetClone()).ToList();

                var entry = new ItemStockEntry
                {
                    ItemsJson = YmmJson.GetJsonText(clones),
                    Summary = BuildSummary(items),
                    ItemCount = items.Count,
                };

                entry.Name = string.IsNullOrWhiteSpace(name) ? MakeUniqueName(entry.Summary) : name;

                return entry;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to serialize the stocked items: {ex}");
                return null;
            }
        }

        public static bool Overwrite(ItemStockEntry entry, IReadOnlyList<IItem> items)
        {
            if (items.Count == 0)
                return false;

            try
            {
                var clones = items.Select(item => item.GetClone()).ToList();

                entry.ItemsJson = YmmJson.GetJsonText(clones);
                entry.Summary = BuildSummary(items);
                entry.ItemCount = items.Count;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to serialize the stocked items: {ex}");
                return false;
            }
        }

        public static List<IItem>? Load(ItemStockEntry entry)
        {
            if (string.IsNullOrEmpty(entry.ItemsJson))
                return null;

            try
            {
                var items = YmmJson.LoadFromText<List<IItem>>(entry.ItemsJson);
                return items is null ? null : [.. items.OfType<IItem>()];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to restore the stocked items: {ex}");
                return null;
            }
        }

        public static string MakeUniqueName(string baseName)
        {
            var trimmed = string.IsNullOrWhiteSpace(baseName) ? "保存したアイテム" : baseName;
            var existing = Entries.Select(entry => entry.Name).ToHashSet(StringComparer.Ordinal);

            if (existing.Add(trimmed))
                return trimmed;

            for (var index = 2; index < 1000; index++)
            {
                var candidate = $"{trimmed} {index}";
                if (!existing.Contains(candidate))
                    return candidate;
            }

            return trimmed;
        }

        public static string BuildSummary(IReadOnlyList<IItem> items)
        {
            var label = SafeLabel(items[0]);
            return items.Count == 1 ? label : $"{label} 他{items.Count - 1}件";
        }

        private static string SafeLabel(IItem item)
        {
            try
            {
                return string.IsNullOrWhiteSpace(item.Label) ? item.GetType().Name : item.Label;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to read the item label: {ex}");
                return item.GetType().Name;
            }
        }
    }
}
