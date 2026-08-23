using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.TachiePlacer.Settings
{
    public enum TachiePlacement
    {
        [Display(Name = nameof(Texts.TachiePlacementBelow), Description = nameof(Texts.TachiePlacementBelowDescription), ResourceType = typeof(Texts))]
        Below = 0,

        [Display(Name = nameof(Texts.TachiePlacementAbove), Description = nameof(Texts.TachiePlacementAboveDescription), ResourceType = typeof(Texts))]
        Above = 1
    }

    public class TachiePlacerSettings : ToolItemSettings
    {
        private TachiePlacement placement = TachiePlacement.Below;
        public TachiePlacement Placement { get => placement; set => Set(ref placement, value); }

        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(1, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "TachiePlacer";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.AccountPlus;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.TachiePlacerDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.TachiePlacerDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("TachiePlacerItemCommand");
    }
}
