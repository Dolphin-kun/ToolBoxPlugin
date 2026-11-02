using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.ItemEditor.CustomVisibilityAttributes;
using YukkuriMovieMaker.Controls;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows.Controls;
using ToolBox.Settings;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Decorations.Settings
{
    public enum SeparatorDirection
    {
        [Display(Name = nameof(Texts.SeparatorHorizontal), ResourceType = typeof(Texts))]
        Horizontal,
        [Display(Name = nameof(Texts.SeparatorVertical), ResourceType = typeof(Texts))]
        Vertical,
    }

    public class SeparatorSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Decoration;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Separator";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.SeparatorDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.SeparatorDescription;

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Minus;

        [JsonIgnore, NJsonIgnore]
        public override bool IsInvokable => false;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary =>
            $"{DisplayName} ({(Orientation == Orientation.Horizontal ? Texts.SeparatorHorizontal : Texts.SeparatorVertical)})";

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation
        {
            get => orientation;
            set => Set(ref orientation, value, etcChangedPropertyNames: [nameof(InstanceSummary), nameof(Direction)]);
        }

        [JsonIgnore, NJsonIgnore]
        [Display(GroupName = nameof(Texts.SeparatorDisplayName), Name = nameof(Texts.SeparatorOrientationLabel), Order = 0, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public SeparatorDirection Direction
        {
            get => Orientation == Orientation.Vertical ? SeparatorDirection.Vertical : SeparatorDirection.Horizontal;
            set => Orientation = value == SeparatorDirection.Vertical ? Orientation.Vertical : Orientation.Horizontal;
        }

        private double lineThickness = 1;
        [Display(GroupName = nameof(Texts.SeparatorDisplayName), Name = nameof(Texts.SeparatorThicknessLabel), Order = 1, Description = nameof(Texts.SeparatorColorHint), ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "px", 1, 10, PropertyEditorSize = PropertyEditorSize.FullWidth)]
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
