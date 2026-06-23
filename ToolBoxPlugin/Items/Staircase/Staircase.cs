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
        public override string IconPath => "M16,10V7A1,1 0 0,0 15,6H4A1,1 0 0,0 3,7V10A1,1 0 0,0 4,11H15A1,1 0 0,0 16,10M16,11H20A1,1 0 0,1 21,12V15A1,1 0 0,1 20,16H4A1,1 0 0,1 3,15V12A1,1 0 0,1 4,11M16,16H20A1,1 0 0,1 21,17V20A1,1 0 0,1 20,21H4A1,1 0 0,1 3,20V17A1,1 0 0,1 4,16";

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("StaircaseCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => StaircaseCommand.Staircase(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
            new("ReverseStaircaseCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => StaircaseCommand.ReverseStaircase(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
