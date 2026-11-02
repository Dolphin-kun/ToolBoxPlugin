namespace ToolBox.Commons
{
    public sealed record PlacementPosition(string Name, HorizontalAnchor Horizontal, VerticalAnchor Vertical, string Label)
    {
        public bool IsCenter => Horizontal == HorizontalAnchor.Center && Vertical == VerticalAnchor.Center;

        public static IReadOnlyList<PlacementPosition> All { get; } =
        [
            new("TopLeft", HorizontalAnchor.Left, VerticalAnchor.Top, Texts.PositionTopLeft),
            new("Top", HorizontalAnchor.Center, VerticalAnchor.Top, Texts.PositionTop),
            new("TopRight", HorizontalAnchor.Right, VerticalAnchor.Top, Texts.PositionTopRight),
            new("Left", HorizontalAnchor.Left, VerticalAnchor.Center, Texts.PositionLeft),
            new("Center", HorizontalAnchor.Center, VerticalAnchor.Center, Texts.PositionCenter),
            new("Right", HorizontalAnchor.Right, VerticalAnchor.Center, Texts.PositionRight),
            new("BottomLeft", HorizontalAnchor.Left, VerticalAnchor.Bottom, Texts.PositionBottomLeft),
            new("Bottom", HorizontalAnchor.Center, VerticalAnchor.Bottom, Texts.PositionBottom),
            new("BottomRight", HorizontalAnchor.Right, VerticalAnchor.Bottom, Texts.PositionBottomRight),
        ];
    }
}
