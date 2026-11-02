using System.Globalization;
using ToolBox.Commons;
using ToolBox.Controls;

namespace ToolBox.Items.AnchorPoint.Views
{
    public sealed class AnchorPointItem : CommandPadView
    {
        private const double DotRadius = 1.6;

        private static readonly System.Windows.Media.Geometry Frame = Glyph("M1,1 H9 V9 H1 Z M1.8,1.8 V8.2 H8.2 V1.8 Z");

        private static readonly IReadOnlyList<CommandPadCell> cells =
        [
            .. PlacementPosition.All.Select(position => CommandPadCell.FromLayers(
                AnchorPointCommand.CommandName(position),
                string.Format(Texts.AnchorPointCommandName, position.Label),
                [(Frame, 0.45), (Dot(position), 1.0)])),
        ];

        protected override IReadOnlyList<CommandPadCell> Cells => cells;

        protected override int PrimaryIndex => 4;

        protected override int? FixedColumns => 3;

        private static System.Windows.Media.Geometry Dot(PlacementPosition position)
        {
            var x = position.Horizontal switch { HorizontalAnchor.Left => 1.4, HorizontalAnchor.Right => 8.6, _ => 5.0 };
            var y = position.Vertical switch { VerticalAnchor.Top => 1.4, VerticalAnchor.Bottom => 8.6, _ => 5.0 };
            return Glyph(string.Create(CultureInfo.InvariantCulture,
                $"M{x - DotRadius},{y} a{DotRadius},{DotRadius} 0 1 0 {DotRadius * 2},0 a{DotRadius},{DotRadius} 0 1 0 {-DotRadius * 2},0 Z"));
        }
    }
}
