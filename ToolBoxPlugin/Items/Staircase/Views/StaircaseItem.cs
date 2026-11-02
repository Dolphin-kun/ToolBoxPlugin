using ToolBox.Controls;

namespace ToolBox.Items.Staircase.Views
{
    public sealed class StaircaseItem : CommandPadView
    {
        private static readonly IReadOnlyList<CommandPadCell> cells =
        [
            CommandPadCell.FromPath("StaircaseItemCommand", Texts.StaircaseDisplayName, "M2 4H17V6H2M0 8H15V10H0M4 0H19V2H4"),
            CommandPadCell.FromPath("ReverseStaircaseItemCommand", Texts.StaircaseReverseCommandName, "M2 4H17V6H2M4 8H19V10H4M0 0H15V2H0"),
        ];

        protected override IReadOnlyList<CommandPadCell> Cells => cells;
    }
}
