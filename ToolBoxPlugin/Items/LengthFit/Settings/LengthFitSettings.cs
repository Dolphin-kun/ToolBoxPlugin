using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.LengthFit.Settings
{
    public class LengthFitSettings : ToolItemSettings
    {
        private int gap;
        [Display(GroupName = nameof(Texts.LengthFitDisplayName), Name = nameof(Texts.LengthFitGapLabel), Description = nameof(Texts.LengthFitGapDescription), Order = 0, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "F", 0, 30, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public int Gap { get => gap; set => Set(ref gap, Math.Max(0, value)); }

        public LengthFitSettings()
        {
            Size = new Size(4, 1);
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "LengthFit";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.ArrowExpandHorizontal;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.LengthFitDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.LengthFitDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault(LengthFitCommand.ToNextCommandName);
    }
}
