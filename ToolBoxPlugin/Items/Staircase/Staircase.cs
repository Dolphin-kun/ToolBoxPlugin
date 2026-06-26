using ToolBox.Items.Staircase.Settings;
using ToolBox.Items.Staircase.Views;

namespace ToolBox.Items.Staircase
{
    public sealed class Staircase : DraggableItem
    {
        public override Type SettingsType => typeof(StaircaseSettings);
        public override Type ViewType => typeof(StaircaseItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(StaircaseSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("StaircaseItemCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => StaircaseItemCommand.Staircase(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
            new("ReverseStaircaseItemCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => StaircaseItemCommand.ReverseStaircase(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
