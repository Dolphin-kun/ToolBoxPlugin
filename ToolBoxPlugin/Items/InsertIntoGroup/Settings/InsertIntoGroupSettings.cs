using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.InsertIntoGroup.Settings
{
    public class InsertIntoGroupSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Layout;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Group";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Group;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.InsertIntoGroupDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.InsertIntoGroupDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("InsertIntoGroupItemCommand");
    }
}
