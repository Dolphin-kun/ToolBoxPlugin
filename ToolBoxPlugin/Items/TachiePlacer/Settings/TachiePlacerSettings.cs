using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.TachiePlacer.Settings
{
    public enum TachiePlacement
    {
        [Display(Name = "下のレイヤー", Description = "音声の1つ下のレイヤー")]
        Below = 0,
        
        [Display(Name = "上のレイヤー", Description = "音声の1つ上のレイヤー")]
        Above = 1
    }

    public class TachiePlacerSettings : ToolItemSettings
    {
        private TachiePlacement placement = TachiePlacement.Below;
        public TachiePlacement Placement { get => placement; set => Set(ref placement, value); }

        [JsonIgnore]
        public override Size MinSize => new(1, 1);

        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore]
        public override string Name => "TachiePlacer";

        [JsonIgnore]
        public override string DisplayName => "立ち絵配置";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("AddTachieItemCommand");
    }
}
