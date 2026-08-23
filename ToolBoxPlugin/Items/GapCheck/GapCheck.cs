using ToolBox.Commons;
using ToolBox.Items.GapCheck.Settings;
using ToolBox.Items.GapCheck.Views;

namespace ToolBox.Items.GapCheck
{
    public sealed class GapCheck : DraggableItem
    {
        public override Type SettingsType => typeof(GapCheckSettings);
        public override Type ViewType => typeof(GapCheckItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(GapCheckSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("GapCheckNextCommand",
                ctx => GapCheckCommand.CanExecute(ctx.Timeline),
                GapCheckCommand.MoveToNext,
                displayName: Texts.GapCheckNextCommandName),
            new("GapCheckPreviousCommand",
                ctx => GapCheckCommand.CanExecute(ctx.Timeline),
                GapCheckCommand.MoveToPrevious,
                displayName: Texts.GapCheckPreviousCommandName),
            new("GapCheckSummaryCommand",
                ctx => GapCheckCommand.CanExecute(ctx.Timeline),
                GapCheckCommand.ShowSummary,
                displayName: Texts.GapCheckSummaryCommandName),
        ];
    }
}
