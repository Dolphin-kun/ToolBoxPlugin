using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ToolBox.Commons;
using ToolBox.Settings;
using YukkuriMovieMaker.Resources.Icons;

namespace ToolBox.Controls
{
    public sealed class RadialMenu : Canvas
    {
        public const double OuterRadius = 138;
        public const double InnerRadius = 58;

        private const double StartAngle = -90;

        private const double ContentWidth = 96;
        private const double PagerContentWidth = 52;
        private const double IconSize = 26;

        private const double DisabledOpacity = 0.3;

        private const double HighlightOverlayOpacity = 0.35;

        public event Action<RadialEntry>? EntryChosen;
        public event Action? BackRequested;

        private readonly Brush sectorBrush;
        private readonly Brush highlightBrush;
        private readonly Brush separatorBrush;
        private readonly Brush foregroundBrush;

        public const int PageSize = 8;

        public IReadOnlyList<RadialEntry> Entries { get; }

        private readonly bool showAvailability;
        private readonly bool hasParent;

        private readonly bool imageFillsSector = ToolBoxSettings.Default.RadialLauncherImageFillsSector;

        private int page;

        private bool IsPaged => Entries.Count > PageSize;

        private int PageCount => Math.Max(1, ((Entries.Count - 1) / PageSize) + 1);

        public RadialMenu(IReadOnlyList<RadialEntry> entries, bool hasParent, bool showAvailability = true)
        {
            Entries = entries;
            this.hasParent = hasParent;
            this.showAvailability = showAvailability;

            sectorBrush = FindBrush(SystemColors.ControlBrushKey) ?? CreateBrush(0x2B, 0x2B, 0x2B);
            separatorBrush = FindBrush(SystemColors.ActiveBorderBrushKey) ?? CreateBrush(0x1A, 0x1A, 0x1A);
            foregroundBrush = FindBrush(SystemColors.ControlTextBrushKey) ?? Brushes.White;
            highlightBrush = CreateHighlight(sectorBrush);

            Resources[YukkuriMovieMaker.Theme.YMM4Colors.IconBrushKey] = foregroundBrush;

            Width = OuterRadius * 2;
            Height = OuterRadius * 2;

            Build();
        }

        private void Build()
        {
            Children.Clear();

            var visible = IsPaged
                ? Entries.Skip(page * PageSize).Take(PageSize).ToList()
                : Entries;

            var center = new Point(OuterRadius, OuterRadius);
            var step = 360.0 / Math.Max(1, visible.Count);

            for (var i = 0; i < visible.Count; i++)
            {
                AddSector(visible[i], center, StartAngle + (i * step), step);
            }

            for (var i = 0; i < visible.Count; i++)
            {
                AddContent(visible[i], center, StartAngle + ((i + 0.5) * step));
            }

            if (IsPaged)
                AddPager(center);
            else
                AddCenter(center, hasParent);
        }

        private void TurnPage(int delta)
        {
            page = ((page + delta) % PageCount + PageCount) % PageCount;
            Build();
        }

        private void AddSector(RadialEntry entry, Point center, double startAngle, double sweepAngle)
        {
            var isAvailable = !showAvailability || entry.IsAvailable;
            var geometry = CreateSectorGeometry(center, startAngle, sweepAngle);

            var sector = new Path
            {
                Data = geometry,
                Fill = sectorBrush,
                Stroke = separatorBrush,
                StrokeThickness = 1.5,
                Cursor = isAvailable ? Cursors.Hand : Cursors.Arrow,
            };

            sector.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                if (isAvailable)
                    EntryChosen?.Invoke(entry);
            };

            Children.Add(sector);

            if (!IsFilledWithImage(entry))
            {
                sector.MouseEnter += (_, _) => sector.Fill = highlightBrush;
                sector.MouseLeave += (_, _) => sector.Fill = sectorBrush;
                return;
            }

            AddSectorImage(entry, geometry, isAvailable);

            var outline = new Path
            {
                Data = geometry,
                Stroke = separatorBrush,
                StrokeThickness = 1.5,
                IsHitTestVisible = false,
            };

            var overlay = new Path
            {
                Data = geometry,
                Fill = highlightBrush,
                Opacity = 0,
                IsHitTestVisible = false,
            };

            sector.MouseEnter += (_, _) => overlay.Opacity = HighlightOverlayOpacity;
            sector.MouseLeave += (_, _) => overlay.Opacity = 0;

            Children.Add(overlay);
            Children.Add(outline);
        }

        private void AddSectorImage(RadialEntry entry, Geometry geometry, bool isAvailable)
        {
            var bounds = geometry.Bounds;

            var image = new AnimatedImage
            {
                SourcePath = entry.ImagePath,
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var clip = geometry.Clone();
            clip.Transform = new TranslateTransform(-bounds.X, -bounds.Y);

            var layer = new Grid
            {
                Width = bounds.Width,
                Height = bounds.Height,
                ClipToBounds = true,
                Clip = clip,
                IsHitTestVisible = false,
                Opacity = isAvailable ? 1 : DisabledOpacity,
            };

            layer.Children.Add(image);

            SetLeft(layer, bounds.X);
            SetTop(layer, bounds.Y);

            Children.Add(layer);
        }

        private bool IsFilledWithImage(RadialEntry entry) =>
            imageFillsSector && !string.IsNullOrWhiteSpace(entry.ImagePath);

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
                    hasParent ? Texts.RadialMenuBack : Texts.RadialMenuClose,
                    hasParent ? IconKeys.ArrowLeft : IconKeys.Close,
                    foregroundBrush,
                    isAvailable: true),
                center);
        }

        private void AddPager(Point center)
        {
            AddPagerHalf(center, isPrevious: true);
            AddPagerHalf(center, isPrevious: false);

            var indicator = new TextBlock
            {
                Text = $"{page + 1} / {PageCount}",
                Foreground = foregroundBrush,
                FontSize = 10,
                Opacity = 0.7,
                TextAlignment = TextAlignment.Center,
                Width = PagerContentWidth * 2,
                IsHitTestVisible = false,
            };

            indicator.Measure(new Size(PagerContentWidth * 2, double.PositiveInfinity));
            SetLeft(indicator, center.X - PagerContentWidth);
            SetTop(indicator, center.Y + (InnerRadius / 2) - (indicator.DesiredSize.Height / 2));

            Children.Add(indicator);
        }

        private void AddPagerHalf(Point center, bool isPrevious)
        {
            var startAngle = isPrevious ? 90.0 : -90.0;

            var half = new Path
            {
                Data = CreateHalfCircleGeometry(center, startAngle),
                Fill = sectorBrush,
                Stroke = separatorBrush,
                StrokeThickness = 1.5,
                Cursor = Cursors.Hand,
            };

            half.MouseEnter += (_, _) => half.Fill = highlightBrush;
            half.MouseLeave += (_, _) => half.Fill = sectorBrush;
            half.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                TurnPage(isPrevious ? -1 : 1);
            };

            Children.Add(half);

            var panel = BuildContentPanel(
                isPrevious ? Texts.RadialMenuPrevious : Texts.RadialMenuNext,
                isPrevious ? IconKeys.ChevronLeft : IconKeys.ChevronRight,
                foregroundBrush,
                isAvailable: true,
                width: PagerContentWidth);

            panel.Measure(new Size(PagerContentWidth, double.PositiveInfinity));

            SetLeft(panel, center.X + (isPrevious ? -InnerRadius / 2 : InnerRadius / 2) - (PagerContentWidth / 2));
            SetTop(panel, center.Y - (panel.DesiredSize.Height / 2) - 6);

            Children.Add(panel);
        }

        private void AddContent(RadialEntry entry, Point center, double angle)
        {
            if (IsFilledWithImage(entry))
                return;

            var panel = BuildContentPanel(
                entry.IsFolder ? entry.Label + " ▸" : entry.Label,
                string.IsNullOrWhiteSpace(entry.IconPath) && entry.IsFolder ? IconKeys.Folder : entry.IconPath,
                entry.Foreground ?? foregroundBrush,
                !showAvailability || entry.IsAvailable,
                imagePath: entry.ImagePath,
                imageStretch: entry.ImageStretch);

            AddContentPanel(panel, PointOnCircle(center, (InnerRadius + OuterRadius) / 2, angle));
        }

        private static StackPanel BuildContentPanel(
            string label, string iconPath, Brush foreground, bool isAvailable,
            double width = ContentWidth, string imagePath = "", Stretch imageStretch = Stretch.Uniform)
        {
            var panel = new StackPanel
            {
                Width = width,
                IsHitTestVisible = false,
                Opacity = isAvailable ? 1 : DisabledOpacity,
            };

            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                panel.Children.Add(new Border
                {
                    Width = IconSize,
                    Height = IconSize,
                    ClipToBounds = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 5),
                    Child = new AnimatedImage
                    {
                        SourcePath = imagePath,
                        Stretch = imageStretch,
                    },
                });
            }
            else if (!string.IsNullOrWhiteSpace(iconPath))
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
                FontSize = width < ContentWidth ? 10 : 11,
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

        private static Geometry CreateRingGeometry(Point center)
        {
            var ring = new CombinedGeometry(
                GeometryCombineMode.Exclude,
                new EllipseGeometry(center, OuterRadius, OuterRadius),
                new EllipseGeometry(center, InnerRadius, InnerRadius));

            ring.Freeze();
            return ring;
        }

        private static Geometry CreateHalfCircleGeometry(Point center, double startAngle)
        {
            var figure = new PathFigure
            {
                StartPoint = PointOnCircle(center, InnerRadius, startAngle),
                IsClosed = true,
                IsFilled = true,
            };

            figure.Segments.Add(new ArcSegment(
                PointOnCircle(center, InnerRadius, startAngle + 180),
                new Size(InnerRadius, InnerRadius),
                0, false, SweepDirection.Clockwise, true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.Freeze();
            return geometry;
        }

        private static Geometry CreateSectorGeometry(Point center, double startAngle, double sweepAngle)
        {
            if (sweepAngle >= 360)
                return CreateRingGeometry(center);

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
