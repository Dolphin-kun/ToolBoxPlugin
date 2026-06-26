using ToolBox.Items.Sort.Settings;
using ToolBox.Items.Sort.Views;

namespace ToolBox.Items.Sort
{
    public sealed class Sort : DraggableItem
    {
        public override Type SettingsType => typeof(SortSettings);
        public override Type ViewType => typeof(SortItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("SortAscendingItemCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => SortCommand.SortByFrameAscending(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
            new("SortDescendingItemCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => SortCommand.SortByFrameDescending(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
