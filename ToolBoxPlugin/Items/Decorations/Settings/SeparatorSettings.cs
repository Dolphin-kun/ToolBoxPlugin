using System.Text.Json.Serialization;
using System.Windows.Controls;
using ToolBox.Settings;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Decorations.Settings
{
    public class SeparatorSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Decoration;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Separator";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => "区切り線";

        [JsonIgnore, NJsonIgnore]
        public override string Description => "パネルに区切り線を引きます。アイテムのまとまりを分けるための飾りです。";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Minus;

        [JsonIgnore, NJsonIgnore]
        public override bool IsInvokable => false;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary =>
            $"{DisplayName} ({(Orientation == Orientation.Horizontal ? "横" : "縦")})";

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation
        {
            get => orientation;
            set => Set(ref orientation, value, etcChangedPropertyNames: nameof(InstanceSummary));
        }

        private double lineThickness = 1;
        public double LineThickness
        {
            get => lineThickness;
            set => Set(ref lineThickness, value);
        }

        public SeparatorSettings()
        {
            DisplayMode = ToolItemDisplayMode.Icon;
        }
    }
}
