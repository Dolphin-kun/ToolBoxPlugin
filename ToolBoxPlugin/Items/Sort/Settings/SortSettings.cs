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
        public override string DisplayName => Texts.SortDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.SortDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("SortAscendingItemCommand");
    }
}
