using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Staircase.Settings
{
    public class StaircaseSettings : ToolItemSettings
    {
        private int offset = 20;
        public int Offset { get => offset; set => Set(ref offset, Math.Max(1, value)); }

        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(2, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Staircase";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.SortReverseVariant;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => "階段状に配置";

        [JsonIgnore, NJsonIgnore]
        public override string Description => "選択したアイテムをレイヤー順に一定フレームずつずらして、階段状に配置します。";

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("StaircaseItemCommand");
    }
}
