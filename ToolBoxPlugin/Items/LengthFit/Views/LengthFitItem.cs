using ToolBox.Controls;
using YukkuriMovieMaker.Resources.Icons;

namespace ToolBox.Items.LengthFit.Views
{
    public sealed class LengthFitItem : CommandPadView
    {
        private static readonly IReadOnlyList<CommandPadCell> cells =
        [
            CommandPadCell.FromIcon(LengthFitCommand.ToNextCommandName, Texts.LengthFitToNextCommandName, IconKeys.ArrowExpandRight),
            CommandPadCell.FromIcon(LengthFitCommand.LongestCommandName, Texts.LengthFitLongestCommandName, IconKeys.ArrowExpandHorizontal),
            CommandPadCell.FromIcon(LengthFitCommand.ShortestCommandName, Texts.LengthFitShortestCommandName, IconKeys.ArrowCollapseHorizontal),
            CommandPadCell.FromIcon(LengthFitCommand.ToSeekCommandName, Texts.LengthFitToSeekCommandName, IconKeys.PageLast),
        ];

        protected override IReadOnlyList<CommandPadCell> Cells => cells;
    }
}
