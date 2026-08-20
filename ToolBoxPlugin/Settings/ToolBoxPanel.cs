using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Settings
{
    public class ToolBoxPanel : Bindable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
        };

        public ObservableCollection<ToolItemSettings> Items { get; } = [];

        private string panelName = string.Empty;
        public string PanelName
        {
            get => panelName;
            set => Set(ref panelName, value);
        }

        private int gridRowCount = 6;
        public int GridRowCount
        {
            get => gridRowCount;
            set => Set(ref gridRowCount, value);
        }

        private int gridColumnCount = 6;
        public int GridColumnCount
        {
            get => gridColumnCount;
            set => Set(ref gridColumnCount, value);
        }

        public Dictionary<string, Point> HiddenItemPositions { get; } = [];

        public Dictionary<string, ToolItemSettings> HiddenItems { get; } = [];

        public void HideItem(ToolItemSettings settings)
        {
            if (!Items.Remove(settings))
                return;

            HiddenItems[settings.HiddenKey] = settings;
            HiddenItemPositions[settings.HiddenKey] = settings.Position;
        }

        public ToolItemSettings? TakeHiddenItem(string key)
        {
            if (!HiddenItems.Remove(key, out var settings))
                return null;

            HiddenItemPositions.Remove(key);
            return settings;
        }

        public static ToolBoxPanel CreateDefault()
        {
            var template = ToolBoxSettings.Default.DefaultPanel;
            var panel = new ToolBoxPanel
            {
                GridRowCount = template.GridRowCount,
                GridColumnCount = template.GridColumnCount,
            };

            foreach (var item in CloneItems(template.Items))
                panel.Items.Add(item);

            foreach (var (key, position) in template.HiddenItemPositions)
                panel.HiddenItemPositions[key] = position;

            return panel;
        }

        public string ToJson() => JsonSerializer.Serialize(new PanelState
        {
            PanelName = PanelName,
            Items = [.. Items],
            GridRowCount = GridRowCount,
            GridColumnCount = GridColumnCount,
            HiddenItemPositions = new Dictionary<string, Point>(HiddenItemPositions),
            HiddenItems = HiddenItems.ToDictionary(pair => pair.Key, pair => (ToolItemSettings?)pair.Value),
        }, JsonOptions);

        public void LoadJson(string json)
        {
            PanelState? state;
            try
            {
                state = JsonSerializer.Deserialize<PanelState>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load panel state: {ex}");
                return;
            }

            if (state == null)
                return;

            PanelName = state.PanelName;
            GridRowCount = state.GridRowCount;
            GridColumnCount = state.GridColumnCount;

            Items.Clear();
            foreach (var item in state.Items.OfType<ToolItemSettings>())
                Items.Add(item);

            HiddenItemPositions.Clear();
            foreach (var (key, position) in state.HiddenItemPositions)
                HiddenItemPositions[key] = position;

            HiddenItems.Clear();
            foreach (var (key, settings) in state.HiddenItems)
            {
                if (settings != null)
                    HiddenItems[key] = settings;
            }
        }

        public void RestoreLayout(string json)
        {
            PanelState? state;
            try
            {
                state = JsonSerializer.Deserialize<PanelState>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to restore the panel layout: {ex}");
                return;
            }

            if (state == null)
                return;

            PanelName = state.PanelName;
            GridRowCount = state.GridRowCount;
            GridColumnCount = state.GridColumnCount;

            var restored = state.Items.OfType<ToolItemSettings>().ToList();
            var restoredById = new Dictionary<Guid, ToolItemSettings>();
            foreach (var item in restored)
                restoredById.TryAdd(item.Id, item);

            for (var index = Items.Count - 1; index >= 0; index--)
            {
                if (!restoredById.ContainsKey(Items[index].Id))
                    Items.RemoveAt(index);
            }

            var currentById = new Dictionary<Guid, ToolItemSettings>();
            foreach (var item in Items)
                currentById.TryAdd(item.Id, item);

            foreach (var item in restored)
            {
                if (currentById.TryGetValue(item.Id, out var existing))
                {
                    existing.Position = item.Position;
                    existing.Size = item.Size;
                    existing.IsLocked = item.IsLocked;
                }
                else
                {
                    Items.Add(item);
                }
            }

            HiddenItemPositions.Clear();
            foreach (var (key, position) in state.HiddenItemPositions)
                HiddenItemPositions[key] = position;

            HiddenItems.Clear();
            foreach (var (key, settings) in state.HiddenItems)
            {
                if (settings != null)
                    HiddenItems[key] = settings;
            }
        }

        private static List<ToolItemSettings> CloneItems(IEnumerable<ToolItemSettings> source)
        {
            var json = JsonSerializer.Serialize(source.ToList(), JsonOptions);
            var clones = JsonSerializer.Deserialize<List<ToolItemSettings?>>(json, JsonOptions) ?? [];
            return [.. clones.OfType<ToolItemSettings>()];
        }

        private sealed class PanelState
        {
            public string PanelName { get; set; } = string.Empty;
            public List<ToolItemSettings?> Items { get; set; } = [];
            public int GridRowCount { get; set; } = 6;
            public int GridColumnCount { get; set; } = 6;
            public Dictionary<string, Point> HiddenItemPositions { get; set; } = [];
            public Dictionary<string, ToolItemSettings?> HiddenItems { get; set; } = [];
        }
    }
}
