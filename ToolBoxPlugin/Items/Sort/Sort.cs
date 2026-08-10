using ToolBox.Commons;
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
                ctx => SortCommand.SortByFrameAscending(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager),
                displayName: "早い順に整列"),
            new("SortDescendingItemCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => SortCommand.SortByFrameDescending(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager),
                displayName: "遅い順に整列"),
        ];
    }
}
