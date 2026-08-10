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

        public static ToolBoxPanel CreateDefault()
        {
            var settings = ToolBoxSettings.Default;
            var panel = new ToolBoxPanel
            {
                GridRowCount = settings.GridRowCount,
                GridColumnCount = settings.GridColumnCount,
            };

            foreach (var item in CloneItems(settings.Items))
                panel.Items.Add(item);

            foreach (var (key, position) in settings.HiddenItemPositions)
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
        }
    }
}
