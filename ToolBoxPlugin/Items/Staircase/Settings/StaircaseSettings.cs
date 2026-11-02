using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.ItemEditor.CustomVisibilityAttributes;
using YukkuriMovieMaker.Controls;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Staircase.Settings
{
    public enum StaircaseOrder
    {
        [Display(Name = nameof(Texts.StaircaseOrderLayer), ResourceType = typeof(Texts))]
        Layer,
        [Display(Name = nameof(Texts.StaircaseOrderCenterOut), ResourceType = typeof(Texts))]
        CenterOut,
        [Display(Name = nameof(Texts.StaircaseOrderEdgesIn), ResourceType = typeof(Texts))]
        EdgesIn,
        [Display(Name = nameof(Texts.StaircaseOrderRandom), ResourceType = typeof(Texts))]
        Random,
    }

    public class StaircaseSettings : ToolItemSettings
    {
        private int offset = 20;
        [Display(GroupName = nameof(Texts.StaircaseDisplayName), Name = nameof(Texts.StaircaseOffsetLabel), Order = 0, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "F", 0, 50, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public int Offset { get => offset; set => Set(ref offset, Math.Max(1, value)); }

        private int amount = 1;
        [Display(GroupName = nameof(Texts.StaircaseDisplayName), Name = nameof(Texts.StaircaseAmountLabel), Order = 1, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "", 1, 10, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public int Amount { get => amount; set => Set(ref amount, Math.Max(1, value)); }

        private StaircaseOrder order = StaircaseOrder.Layer;
        [Display(GroupName = nameof(Texts.StaircaseDisplayName), Name = nameof(Texts.StaircaseOrderLabel), Order = 2, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StaircaseOrder Order { get => order; set => Set(ref order, value); }

        private EasingType curve = EasingType.Linear;
        [Display(GroupName = nameof(Texts.StaircaseDisplayName), Name = nameof(Texts.StaircaseCurveLabel), Order = 3, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EasingType Curve { get => curve; set => Set(ref curve, value, etcChangedPropertyNames: [nameof(IsCurved)]); }

        [JsonIgnore, NJsonIgnore]
        public bool IsCurved => Curve != EasingType.Linear;

        private EasingMode direction = EasingMode.In;
        [Display(GroupName = nameof(Texts.StaircaseDisplayName), Name = nameof(Texts.EasingDirectionLabel), Order = 4, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(IsCurved), true)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EasingMode Direction { get => direction; set => Set(ref direction, value); }

        public StaircaseSettings()
        {
            Size = new Size(2, 1);
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Staircase";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.SortReverseVariant;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.StaircaseDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.StaircaseDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("StaircaseItemCommand");
    }
}
