using ToolBox.Commons;
using ToolBox.Controls;

namespace ToolBox.Items.ScreenAlign.Views
{
    public sealed class ScreenAlignItem : CommandPadView
    {
        private static readonly Dictionary<string, string> glyphs = new()
        {
            ["TopLeft"] = "M1,1 H5 V2 H2 V5 H1 Z",
            ["Top"] = "M3,1 H7 V2 H3 Z",
            ["TopRight"] = "M5,1 H9 V5 H8 V2 H5 Z",
            ["Left"] = "M1,3 H2 V7 H1 Z",
            ["Center"] = "M4,4 H6 V6 H4 Z M4,2.5 H6 V3.1 H4 Z M4,6.9 H6 V7.5 H4 Z M2.5,4 H3.1 V6 H2.5 Z M6.9,4 H7.5 V6 H6.9 Z",
            ["Right"] = "M8,3 H9 V7 H8 Z",
            ["BottomLeft"] = "M1,5 H2 V8 H5 V9 H1 Z",
            ["Bottom"] = "M3,8 H7 V9 H3 Z",
            ["BottomRight"] = "M8,5 H9 V9 H5 V8 H8 Z",
        };

        private static readonly IReadOnlyList<CommandPadCell> cells =
        [
            .. PlacementPosition.All.Select(position => CommandPadCell.FromLayers(
                ScreenAlignCommand.CommandName(position),
                string.Format(Texts.ScreenAlignCommandName, position.Label),
                [(Glyph(glyphs[position.Name]), 1.0)])),
        ];

        protected override IReadOnlyList<CommandPadCell> Cells => cells;

        protected override int PrimaryIndex => 4;

        protected override int? FixedColumns => 3;

        protected override IEnumerable<(string CommandName, string Header)> ExtraMenuItems =>
        [
            (ScreenAlignCommand.DistributeHorizontalCommandName, Texts.ScreenDistributeHorizontalCommandName),
            (ScreenAlignCommand.DistributeVerticalCommandName, Texts.ScreenDistributeVerticalCommandName),
        ];
    }
}
