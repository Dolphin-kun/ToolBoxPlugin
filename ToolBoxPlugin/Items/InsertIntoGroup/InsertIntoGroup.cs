using ToolBox.Items.InsertIntoGroup.Settings;
using ToolBox.Items.InsertIntoGroup.Views;

namespace ToolBox.Items.InsertIntoGroup
{
    public sealed class InsertIntoGroup : DraggableItem
    {
        public override Type SettingsType => typeof(InsertIntoGroupSettings);
        public override Type ViewType => typeof(InsertIntoGroupItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("InsertIntoGroupItemCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => InsertIntoGroupItemCommand.InsertIntoGroup(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
