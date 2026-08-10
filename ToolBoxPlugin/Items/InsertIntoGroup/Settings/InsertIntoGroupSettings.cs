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
        public override string DisplayName => "グループ化";

        [JsonIgnore, NJsonIgnore]
        public override string Description => "選択したアイテム全体を覆うグループ制御を追加して、まとめて扱えるようにします。";

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("InsertIntoGroupItemCommand");
    }
}
