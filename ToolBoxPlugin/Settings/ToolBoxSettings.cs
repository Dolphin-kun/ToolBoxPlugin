using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;
using ToolBox.Commons;
using ToolBox.Items.SeparateAudio.Settings;
using ToolBox.Items.InsertIntoGroup.Settings;
using ToolBox.Items.Ruby.Settings;
using ToolBox.Items.Sort.Settings;
using ToolBox.Items.Staircase.Settings;
using ToolBox.Items.Recording.Settings;
using ToolBox.Items.TextSplitter.Settings;
using ToolBox.Items.TachiePlacer.Settings;
using YukkuriMovieMaker.Plugin;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Settings
{
    public class ToolBoxSettings : SettingsBase<ToolBoxSettings>
    {
        public override SettingsCategory Category => SettingsCategory.Tool;
        public override string Name => Texts.PluginName;
        public override bool HasSettingView => false;
        public override object? SettingView => null;

        #region 全パネル共通の見た目

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

        private bool showGridLines = false;
        public bool ShowGridLines
        {
            get => showGridLines;
            set => Set(ref showGridLines, value);
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

        [JsonIgnore, NJsonIgnore]
        public Color HighlightColor
        {
            get => ColorHelper.Parse(HighlightColorHex, Color.FromArgb(0xFF, 0x00, 0x99, 0xFF));
            set => HighlightColorHex = value.ToString();
        }

        private bool showLockBadge = true;
        public bool ShowLockBadge
        {
            get => showLockBadge;
            set => Set(ref showLockBadge, value);
        }

        #endregion

        #region グリッドの調整

        private GridAdjustMode gridAdjustMode = GridAdjustMode.Manual;
        public GridAdjustMode GridAdjustMode
        {
            get => gridAdjustMode;
            set => Set(ref gridAdjustMode, value);
        }

        private bool autoFitSize = false;
        public bool AutoFitSize
        {
            get => autoFitSize;
            set => Set(ref autoFitSize, value);
        }

        private bool autoFitLayout = false;
        public bool AutoFitLayout
        {
            get => autoFitLayout;
            set => Set(ref autoFitLayout, value);
        }

        #endregion

        #region 円形ランチャー

        private bool isRadialLauncherEnabled = true;
        public bool IsRadialLauncherEnabled
        {
            get => isRadialLauncherEnabled;
            set => Set(ref isRadialLauncherEnabled, value);
        }

        private ObservableCollection<LauncherNode> launcherNodes = [];
        public ObservableCollection<LauncherNode> LauncherNodes
        {
            get => launcherNodes;
            set => Set(ref launcherNodes, value ?? []);
        }

        #endregion

        #region 保存したアイテム

        private ObservableCollection<ItemStockEntry> itemStocks = [];
        public ObservableCollection<ItemStockEntry> ItemStocks
        {
            get => itemStocks;
            set => Set(ref itemStocks, value ?? []);
        }

        #endregion

        private DefaultPanelSettings defaultPanel = new();
        public DefaultPanelSettings DefaultPanel
        {
            get => defaultPanel;
            set => Set(ref defaultPanel, value ?? new DefaultPanelSettings());
        }

        public List<string> DisabledAddonFileNames { get; set; } = [];

        #region v1.1 以前の保存形式。読み込み時に DefaultPanel へ移し替える

        public ObservableCollection<ToolItemSettings>? Items { get; set; }
        public int? GridRowCount { get; set; }
        public int? GridColumnCount { get; set; }
        public Dictionary<string, Point>? HiddenItemPositions { get; set; }

        #endregion

        public override void Initialize()
        {
            MigrateLegacyLayout();
            EnsureDefaultItems();
        }

        private void MigrateLegacyLayout()
        {
            if (Items is { Count: > 0 } legacyItems && DefaultPanel.Items.Count == 0)
                DefaultPanel.Items = [.. legacyItems];

            if (GridRowCount is { } rows)
                DefaultPanel.GridRowCount = rows;

            if (GridColumnCount is { } columns)
                DefaultPanel.GridColumnCount = columns;

            if (HiddenItemPositions is { Count: > 0 } legacyPositions)
            {
                foreach (var (key, position) in legacyPositions)
                    DefaultPanel.HiddenItemPositions.TryAdd(key, position);
            }

            Items = null;
            GridRowCount = null;
            GridColumnCount = null;
            HiddenItemPositions = null;
        }

        private void EnsureDefaultItems()
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

        private void EnsureItemExists<T>(Point defaultPosition) where T : ToolItemSettings, new()
        {
            if (DefaultPanel.Items.OfType<T>().Any())
                return;

            DefaultPanel.Items.Add(new T { Position = defaultPosition });
        }
    }
}
