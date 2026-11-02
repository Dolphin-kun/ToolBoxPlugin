using ToolBox.Controls;
using YukkuriMovieMaker.Resources.Icons;

namespace ToolBox.Items.KeyFrameTools.Views
{
    public sealed class KeyFrameToolsItem : CommandPadView
    {
        private static readonly IReadOnlyList<CommandPadCell> cells =
        [
            CommandPadCell.FromIcon("KeyFrameShiftEarlierCommand", Texts.KeyFrameShiftEarlierCommandName, IconKeys.ArrowLeftBold),
            CommandPadCell.FromIcon("KeyFrameReverseCommand", Texts.KeyFrameReverseCommandName, IconKeys.SwapHorizontal),
            CommandPadCell.FromIcon("KeyFrameShiftLaterCommand", Texts.KeyFrameShiftLaterCommandName, IconKeys.ArrowRightBold),
        ];

        protected override IReadOnlyList<CommandPadCell> Cells => cells;

        protected override int PrimaryIndex => 1;
    }
}
