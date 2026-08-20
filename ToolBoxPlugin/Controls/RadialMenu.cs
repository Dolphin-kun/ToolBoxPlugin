using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ToolBox.Commons;
using YukkuriMovieMaker.Resources.Icons;

namespace ToolBox.Controls
{
    public sealed class RadialMenu : Canvas
    {
        public const double OuterRadius = 138;
        public const double InnerRadius = 58;

        private const double StartAngle = -90;

        private const double ContentWidth = 96;
        private const double IconSize = 26;

        private const double DisabledOpacity = 0.3;

        public event Action<RadialEntry>? EntryChosen;
        public event Action? BackRequested;

        private readonly Brush sectorBrush;
        private readonly Brush highlightBrush;
        private readonly Brush separatorBrush;
        private readonly Brush foregroundBrush;

        public IReadOnlyList<RadialEntry> Entries { get; }

        private readonly bool showAvailability;

        public RadialMenu(IReadOnlyList<RadialEntry> entries, bool hasParent, bool showAvailability = true)
        {
            Entries = entries;
            this.showAvailability = showAvailability;

            sectorBrush = FindBrush(SystemColors.ControlBrushKey) ?? CreateBrush(0x2B, 0x2B, 0x2B);
            separatorBrush = FindBrush(SystemColors.ActiveBorderBrushKey) ?? CreateBrush(0x1A, 0x1A, 0x1A);
            foregroundBrush = FindBrush(SystemColors.ControlTextBrushKey) ?? Brushes.White;
            highlightBrush = CreateHighlight(sectorBrush);

            Resources[YukkuriMovieMaker.Theme.YMM4Colors.IconBrushKey] = foregroundBrush;

            Width = OuterRadius * 2;
            Height = OuterRadius * 2;

            var center = new Point(OuterRadius, OuterRadius);
            var step = 360.0 / Math.Max(1, entries.Count);

            for (var i = 0; i < entries.Count; i++)
            {
                AddSector(entries[i], center, StartAngle + (i * step), step);
            }

            for (var i = 0; i < entries.Count; i++)
            {
                AddContent(entries[i], center, StartAngle + ((i + 0.5) * step));
            }

            AddCenter(center, hasParent);
        }

        private void AddSector(RadialEntry entry, Point center, double startAngle, double sweepAngle)
        {
            var isAvailable = !showAvailability || entry.IsAvailable;

            var sector = new Path
            {
                Data = CreateSectorGeometry(center, startAngle, sweepAngle),
                Fill = sectorBrush,
                Stroke = separatorBrush,
                StrokeThickness = 1.5,
                Cursor = isAvailable ? Cursors.Hand : Cursors.Arrow,
            };

            sector.MouseEnter += (_, _) => sector.Fill = highlightBrush;
            sector.MouseLeave += (_, _) => sector.Fill = sectorBrush;
            sector.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                if (isAvailable)
                    EntryChosen?.Invoke(entry);
            };

            Children.Add(sector);
        }

        private void AddCenter(Point center, bool hasParent)
        {
            var circle = new Ellipse
            {
                Width = InnerRadius * 2,
                Height = InnerRadius * 2,
                Fill = sectorBrush,
                Stroke = separatorBrush,
                StrokeThickness = 1.5,
                Cursor = Cursors.Hand,
            };

            SetLeft(circle, center.X - InnerRadius);
            SetTop(circle, center.Y - InnerRadius);

            circle.MouseEnter += (_, _) => circle.Fill = highlightBrush;
            circle.MouseLeave += (_, _) => circle.Fill = sectorBrush;
            circle.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                BackRequested?.Invoke();
            };

            Children.Add(circle);

            AddContentPanel(
                BuildContentPanel(
                    hasParent ? "戻る" : "閉じる",
                    hasParent ? IconKeys.ArrowLeft : IconKeys.Close,
                    foregroundBrush,
                    isAvailable: true),
                center);
        }

        private void AddContent(RadialEntry entry, Point center, double angle)
        {
            var panel = BuildContentPanel(
                entry.IsFolder ? entry.Label + " ▸" : entry.Label,
                string.IsNullOrWhiteSpace(entry.IconPath) && entry.IsFolder ? IconKeys.Folder : entry.IconPath,
                entry.Foreground ?? foregroundBrush,
                !showAvailability || entry.IsAvailable);

            AddContentPanel(panel, PointOnCircle(center, (InnerRadius + OuterRadius) / 2, angle));
        }

        private static StackPanel BuildContentPanel(string label, string iconPath, Brush foreground, bool isAvailable)
        {
            var panel = new StackPanel
            {
                Width = ContentWidth,
                IsHitTestVisible = false,
                Opacity = isAvailable ? 1 : DisabledOpacity,
            };

            if (!string.IsNullOrWhiteSpace(iconPath))
            {
                panel.Children.Add(new IconControl
                {
                    IconData = iconPath,
                    Foreground = foreground,
                    Width = IconSize,
                    Height = IconSize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5),
                });
            }

            panel.Children.Add(new TextBlock
            {
                Text = label,
                Foreground = foreground,
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            return panel;
        }

        private void AddContentPanel(StackPanel panel, Point position)
        {
            panel.Measure(new Size(ContentWidth, double.PositiveInfinity));

            SetLeft(panel, position.X - (ContentWidth / 2));
            SetTop(panel, position.Y - (panel.DesiredSize.Height / 2));

            Children.Add(panel);
        }

        private static Geometry CreateSectorGeometry(Point center, double startAngle, double sweepAngle)
        {
            var endAngle = startAngle + sweepAngle;
            var isLargeArc = sweepAngle > 180;

            var figure = new PathFigure
            {
                StartPoint = PointOnCircle(center, OuterRadius, startAngle),
                IsClosed = true,
                IsFilled = true,
            };

            figure.Segments.Add(new ArcSegment(
                PointOnCircle(center, OuterRadius, endAngle),
                new Size(OuterRadius, OuterRadius),
                0, isLargeArc, SweepDirection.Clockwise, true));

            figure.Segments.Add(new LineSegment(
                PointOnCircle(center, InnerRadius, endAngle), true));

            figure.Segments.Add(new ArcSegment(
                PointOnCircle(center, InnerRadius, startAngle),
                new Size(InnerRadius, InnerRadius),
                0, isLargeArc, SweepDirection.Counterclockwise, true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.Freeze();
            return geometry;
        }

        private static Point PointOnCircle(Point center, double radius, double degrees)
        {
            var radians = degrees * Math.PI / 180;
            return new Point(
                center.X + (radius * Math.Cos(radians)),
                center.Y + (radius * Math.Sin(radians)));
        }

        private static Brush? FindBrush(object key)
        {
            if (Application.Current?.MainWindow?.TryFindResource(key) is Brush fromWindow)
                return fromWindow;

            return Application.Current?.TryFindResource(key) as Brush;
        }

        private static Brush CreateHighlight(Brush background)
        {
            if (background is not SolidColorBrush solid)
                return background;

            var color = solid.Color;
            var luminance = ((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B)) / 255;
            var shift = luminance < 0.5 ? 28 : -28;

            return CreateBrush(
                Clamp(color.R + shift),
                Clamp(color.G + shift),
                Clamp(color.B + shift));
        }

        private static byte Clamp(int value) => (byte)Math.Clamp(value, 0, 255);

        private static Brush CreateBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }
    }
}
