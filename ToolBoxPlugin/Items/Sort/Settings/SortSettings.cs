using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Sort.Settings
{
    public class SortSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(2, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Sort";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Sort;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => "整列";

        [JsonIgnore, NJsonIgnore]
        public override string Description => "選択したアイテムを開始位置の早い順・遅い順にレイヤーへ並べ直します。";

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("SortAscendingItemCommand");
    }
}
