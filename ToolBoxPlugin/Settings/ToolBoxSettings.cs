using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using ToolBox.Items.SeparateAudio.Settings;
using ToolBox.Items.InsertIntoGroup.Settings;
using ToolBox.Items.Ruby.Settings;
using ToolBox.Items.Sort.Settings;
using ToolBox.Items.Staircase.Settings;
using ToolBox.Items.Recording.Settings;
using ToolBox.Items.TextSplitter.Settings;
using ToolBox.Items.TachiePlacer.Settings;
using YukkuriMovieMaker.Plugin;

namespace ToolBox.Settings
{
    public class ToolBoxSettings : SettingsBase<ToolBoxSettings>
    {
        public override SettingsCategory Category => SettingsCategory.Tool;
        public override string Name => "ツールボックス";
        public override bool HasSettingView => false;
        public override object? SettingView => null;

        private ObservableCollection<ToolItemSettings> items = [];
        public ObservableCollection<ToolItemSettings> Items
        {
            get => items;
            set => Set(ref items, value);
        }

        private int gridColumnCount = 6;
        public int GridColumnCount
        {
            get => gridColumnCount;
            set => Set(ref gridColumnCount, value);
        }

        private int gridRowCount = 6;
        public int GridRowCount
        {
            get => gridRowCount;
            set => Set(ref gridRowCount, value);
        }

        private double cellSize = 40;
        public double CellSize
        {
            get => cellSize;
            set => Set(ref cellSize, value);
        }

        private double cellMargin = 2;
        public double CellMargin
        {
            get => cellMargin;
            set => Set(ref cellMargin, value);
        }

        private bool showHighlight = true;
        public bool ShowHighlight
        {
            get => showHighlight;
            set => Set(ref showHighlight, value);
        }

        private string highlightColorHex = "#FF0099FF";
        public string HighlightColorHex
        {
            get => highlightColorHex;
            set => Set(ref highlightColorHex, value, etcChangedPropertyNames: nameof(HighlightColor));
        }

        [JsonIgnore]
        public Color HighlightColor
        {
            get
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(HighlightColorHex);
                }
                catch
                {
                    return Color.FromArgb(0xFF, 0x00, 0x99, 0xFF);
                }
            }
            set
            {
                HighlightColorHex = value.ToString();
                OnPropertyChanged();
            }
        }

        private bool showGridLines = false;
        public bool ShowGridLines
        {
            get => showGridLines;
            set => Set(ref showGridLines, value);
        }

        public Dictionary<string, Point> HiddenItemPositions { get; set; } = [];

        public override void Initialize()
        {
            Migrate();
        }

        public void Migrate()
        {
            EnsureItemExists<StaircaseSettings>(new Point(0, 0));
            EnsureItemExists<RubySettings>(new Point(1, 0));
            EnsureItemExists<InsertIntoGroupSettings>(new Point(0, 1));
            EnsureItemExists<SortSettings>(new Point(0, 2));
            EnsureItemExists<SeparateAudioSettings>(new Point(2, 1));
            EnsureItemExists<RecordingSettings>(new Point(1, 1));
            EnsureItemExists<TextSplitterSettings>(new Point(3, 1));
            EnsureItemExists<TachiePlacerSettings>(new Point(4, 1));
        }

        private void EnsureItemExists<T>(Point defaultPos, Action<T>? configure = null) where T : ToolItemSettings, new()
        {
            if (!Items.OfType<T>().Any())
            {
                var item = new T { Position = defaultPos };
                configure?.Invoke(item);
                Items.Add(item);
            }
        }
    }
}
