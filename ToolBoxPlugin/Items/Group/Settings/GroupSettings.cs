using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.Group.Settings
{
    public class GroupSettings : ToolItemSettings
    {
        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Layout;

        [JsonIgnore]
        public override string Name => "Group";

        [JsonIgnore]
        public override string DisplayName => "グループ化";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("InsertIntoGroupCommand");
    }
}
