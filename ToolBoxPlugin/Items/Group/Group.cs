using ToolBox.Items.Group.Settings;
using ToolBox.Items.Group.Views;

namespace ToolBox.Items.Group
{
    public sealed class Group : DraggableItem
    {
        public override Type SettingsType => typeof(GroupSettings);
        public override Type ViewType => typeof(GroupItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;
        public override string IconPath => "M4,4H10V10H4V4M20,4V10H14V4H20M14,15H17V13H19V15H22V17H19V19H17V17H14V15M10,14H4V20H10V14Z";

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("InsertIntoGroupCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => GroupCommand.InsertIntoGroup(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
