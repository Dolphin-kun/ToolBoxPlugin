using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.InsertIntoGroup.Settings
{
    public class InsertIntoGroupSettings : ToolItemSettings
    {
        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Layout;

        [JsonIgnore]
        public override string Name => "Group";

        [JsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Group;

        [JsonIgnore]
        public override string DisplayName => "グループ化";

        [JsonIgnore]
        public override string Description => "選択したアイテム全体を覆うグループ制御を追加して、まとめて扱えるようにします。";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("InsertIntoGroupItemCommand");
    }
}
