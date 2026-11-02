using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.ScreenAlign.Settings
{
    public enum AlignReference
    {
        [Display(Name = nameof(Texts.AlignReferenceScreen), ResourceType = typeof(Texts))]
        Screen,
        [Display(Name = nameof(Texts.AlignReferenceSelection), ResourceType = typeof(Texts))]
        Selection,
        [Display(Name = nameof(Texts.AlignReferenceTopItem), ResourceType = typeof(Texts))]
        TopItem,
    }

    public class ScreenAlignSettings : ToolItemSettings
    {
        private AlignReference reference = AlignReference.Screen;
        [Display(GroupName = nameof(Texts.ScreenAlignDisplayName), Name = nameof(Texts.AlignReferenceLabel), Description = nameof(Texts.AlignReferenceDescription), Order = -1, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlignReference Reference { get => reference; set => Set(ref reference, value); }

        private double marginX;
        [Display(GroupName = nameof(Texts.ScreenAlignDisplayName), Name = nameof(Texts.ScreenAlignMarginXLabel), Order = 0, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "px", 0, 200, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public double MarginX { get => marginX; set => Set(ref marginX, value); }

        private double marginY;
        [Display(GroupName = nameof(Texts.ScreenAlignDisplayName), Name = nameof(Texts.ScreenAlignMarginYLabel), Order = 1, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "px", 0, 200, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public double MarginY { get => marginY; set => Set(ref marginY, value); }

        public ScreenAlignSettings()
        {
            Size = new Size(2, 2);
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Animation;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "ScreenAlign";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.AlignHorizontalCenter;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.ScreenAlignDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.ScreenAlignDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("ScreenAlignCenterCommand");
    }
}
