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

        private int amount = 1;
        public int Amount { get => amount; set => Set(ref amount, Math.Max(1, value)); }

        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(2, 1);

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
