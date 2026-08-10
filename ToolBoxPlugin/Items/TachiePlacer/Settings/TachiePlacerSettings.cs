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
        [Display(Name = "下のレイヤー", Description = "音声の1つ下のレイヤー")]
        Below = 0,
        
        [Display(Name = "上のレイヤー", Description = "音声の1つ上のレイヤー")]
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
        public override string DisplayName => "立ち絵配置";

        [JsonIgnore, NJsonIgnore]
        public override string Description => "選択した音声アイテムのキャラクターに対応する立ち絵アイテムを、上下いずれかの隣接レイヤーへ配置します。";

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("TachiePlacerItemCommand");
    }
}
