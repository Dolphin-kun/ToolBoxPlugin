using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ToolBox.Commons;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.LayerEditor.Settings
{
    public class LayerEditorSettings : ToolItemSettings
    {
        public const double DefaultVolume = 100;
        public static readonly Color DefaultLayerColor = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);

        private LayerTargetScope scope = LayerTargetScope.SelectedLayers;
        public LayerTargetScope Scope
        {
            get => scope;
            set => Set(ref scope, value);
        }

        private int fromLayer;
        public int FromLayer
        {
            get => fromLayer;
            set => Set(ref fromLayer, Math.Max(0, value));
        }

        private int toLayer = 5;
        public int ToLayer
        {
            get => toLayer;
            set => Set(ref toLayer, Math.Max(0, value));
        }

        private bool changeEnabled;
        public bool ChangeEnabled
        {
            get => changeEnabled;
            set => Set(ref changeEnabled, value);
        }

        private bool isLayerEnabled = true;
        public bool IsLayerEnabled
        {
            get => isLayerEnabled;
            set => Set(ref isLayerEnabled, value);
        }

        private bool changeLabel = true;
        public bool ChangeLabel
        {
            get => changeLabel;
            set => Set(ref changeLabel, value);
        }

        private string labelTemplate = string.Empty;
        public string LabelTemplate
        {
            get => labelTemplate;
            set => Set(ref labelTemplate, value ?? string.Empty);
        }

        private bool changeColor;
        public bool ChangeColor
        {
            get => changeColor;
            set => Set(ref changeColor, value);
        }

        private string layerColorHex = "#00FFFFFF";
        public string LayerColorHex
        {
            get => layerColorHex;
            set => Set(ref layerColorHex, value, etcChangedPropertyNames: nameof(LayerColor));
        }

        [JsonIgnore, NJsonIgnore]
        public Color LayerColor
        {
            get => ColorHelper.Parse(LayerColorHex, DefaultLayerColor);
            set => LayerColorHex = value.ToString();
        }

        private bool changeVolume;
        public bool ChangeVolume
        {
            get => changeVolume;
            set => Set(ref changeVolume, value);
        }

        private double layerVolume = DefaultVolume;
        public double LayerVolume
        {
            get => layerVolume;
            set => Set(ref layerVolume, Math.Clamp(value, 0, 200));
        }

        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(1, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "LayerEditor";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.LabelMultiple;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.LayerEditorDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.LayerEditorDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("LayerEditorApplyCommand");
    }
}
