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
        public override string IconPath => "M18 21L14 17H17V7H14L18 3L22 7H19V17H22M2 19V17H12V19M2 13V11H9V13M2 7V5H6V7H2Z";

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("SortAscendingCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => SortCommand.SortByFrameAscending(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
            new("SortDescendingCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => SortCommand.SortByFrameDescending(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
