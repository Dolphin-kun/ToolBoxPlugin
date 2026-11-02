using ToolBox.Commons;
using ToolBox.Items.LengthFit.Settings;
using ToolBox.Items.LengthFit.Views;

namespace ToolBox.Items.LengthFit
{
    public sealed class LengthFit : DraggableItem
    {
        public override Type SettingsType => typeof(LengthFitSettings);
        public override Type ViewType => typeof(LengthFitItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new(LengthFitCommand.ToNextCommandName, LengthFitCommand.CanExecute, LengthFitCommand.ToNext,
                displayName: Texts.LengthFitToNextCommandName),
            new(LengthFitCommand.LongestCommandName, LengthFitCommand.CanMatch, LengthFitCommand.Longest,
                displayName: Texts.LengthFitLongestCommandName),
            new(LengthFitCommand.ShortestCommandName, LengthFitCommand.CanMatch, LengthFitCommand.Shortest,
                displayName: Texts.LengthFitShortestCommandName),
            new(LengthFitCommand.ToSeekCommandName, LengthFitCommand.CanExecute, LengthFitCommand.ToSeek,
                displayName: Texts.LengthFitToSeekCommandName),
        ];
    }
}
