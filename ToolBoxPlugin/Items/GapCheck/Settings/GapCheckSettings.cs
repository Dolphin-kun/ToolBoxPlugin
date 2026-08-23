using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.GapCheck.Settings
{
    public class GapCheckSettings : ToolItemSettings
    {
        private int maxGapFrames = 2;
        public int MaxGapFrames
        {
            get => maxGapFrames;
            set => Set(ref maxGapFrames, Math.Max(1, value));
        }

        private GapSearchMode mode = GapSearchMode.All;
        public GapSearchMode Mode
        {
            get => mode;
            set => Set(ref mode, value);
        }

        private bool selectedLayersOnly;
        public bool SelectedLayersOnly
        {
            get => selectedLayersOnly;
            set => Set(ref selectedLayersOnly, value);
        }

        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(3, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "GapCheck";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Magnify;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.GapCheckDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.GapCheckDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("GapCheckNextCommand");
    }
}
