using System.Text.Json.Serialization;
using System.Windows.Controls;
using ToolBox.Settings;

namespace ToolBox.Items.Decorations.Settings
{
    public class SeparatorSettings : ToolItemSettings
    {
        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Decoration;

        [JsonIgnore]
        public override string Name => "Separator";

        [JsonIgnore]
        public override string DisplayName => "区切り線";

        [JsonIgnore]
        public override string Description => "パネルに区切り線を引きます。アイテムのまとまりを分けるための飾りです。";

        [JsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Minus;

        [JsonIgnore]
        public override bool IsInvokable => false;

        [JsonIgnore]
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
