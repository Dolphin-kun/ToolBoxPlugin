using ToolBox.Controls;
using YukkuriMovieMaker.Resources.Icons;

namespace ToolBox.Items.MotionCopy.Views
{
    public sealed class MotionCopyItem : CommandPadView
    {
        private static readonly IReadOnlyList<CommandPadCell> cells =
        [
            CommandPadCell.FromIcon(MotionCopyCommand.CopyCommandName, Texts.MotionCopyCommandName, IconKeys.ContentCopy),
            CommandPadCell.FromIcon(MotionCopyCommand.PasteCommandName, Texts.MotionPasteCommandName, IconKeys.ContentPaste),
        ];

        protected override IReadOnlyList<CommandPadCell> Cells => cells;
    }
}
