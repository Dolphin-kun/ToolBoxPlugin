using System.Windows.Media;

namespace ToolBox.Controls
{
    public sealed class RadialEntry
    {
        public required string Label { get; init; }

        public string IconPath { get; init; } = string.Empty;

        public string ImagePath { get; init; } = string.Empty;

        public Stretch ImageStretch { get; init; } = Stretch.Uniform;

        public IReadOnlyList<RadialEntry> Children { get; init; } = [];

        public Action? Invoke { get; init; }

        public Func<bool>? CanInvoke { get; init; }

        public Brush? Foreground { get; init; }

        public bool IsFolder => Invoke == null;

        public bool IsAvailable => IsFolder
            ? Children.Count > 0
            : CanInvoke?.Invoke() != false;
    }
}
