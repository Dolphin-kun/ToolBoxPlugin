using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ToolBox.Commons;
using ToolBox.Settings;
using YukkuriMovieMaker.Theme;

namespace ToolBox.Controls
{
    public sealed record CommandPadCell(string CommandName, string ToolTip, Func<FrameworkElement> CreateGlyph)
    {
        private const double GlyphSize = 10;

        public static CommandPadCell FromIcon(string commandName, string toolTip, string iconKey) =>
            new(commandName, toolTip, () => new IconControl { IconData = iconKey });

        public static CommandPadCell FromPath(string commandName, string toolTip, string pathData)
        {
            var geometry = CommandPadView.Glyph(pathData);
            return new(commandName, toolTip, () => CreatePath(geometry, 1.0, Stretch.Uniform));
        }

        public static CommandPadCell FromLayers(string commandName, string toolTip, IReadOnlyList<(Geometry Geometry, double Opacity)> layers) =>
            new(commandName, toolTip, () =>
            {
                var canvas = new Grid { Width = GlyphSize, Height = GlyphSize };
                foreach (var (geometry, opacity) in layers)
                    canvas.Children.Add(CreatePath(geometry, opacity, Stretch.None));
                return canvas;
            });

        private static Path CreatePath(Geometry geometry, double opacity, Stretch stretch)
        {
            var path = new Path { Data = geometry, Opacity = opacity, Stretch = stretch };
            path.SetResourceReference(Shape.FillProperty, YMM4Colors.IconBrushKey);
            return path;
        }
    }

    public abstract class CommandPadView : ToolItemView
    {
        private const double MinButtonCells = 0.5;

        protected abstract IReadOnlyList<CommandPadCell> Cells { get; }

        protected virtual int PrimaryIndex => 0;

        protected virtual int? FixedColumns => null;

        protected virtual IEnumerable<(string CommandName, string Header)> ExtraMenuItems => [];

        private (bool Compact, int Columns)? builtLayout;

        protected CommandPadView()
        {
            Loaded += (_, _) => Build();
        }

        protected override void OnCellSpanChanged() => Build();

        private (bool Compact, int Columns) GetLayout()
        {
            var count = Cells.Count;
            IEnumerable<int> candidates = FixedColumns is { } fixedColumns ? [fixedColumns] : Enumerable.Range(1, count).Reverse();

            var (columns, size) = candidates
                .Select(columns => (Columns: columns, Size: Math.Min(CellSpan.Width / columns, CellSpan.Height / Math.Ceiling((double)count / columns))))
                .MaxBy(candidate => candidate.Size);

            return (size < MinButtonCells, columns);
        }

        private void Build()
        {
            var layout = GetLayout();
            if (builtLayout == layout)
                return;

            builtLayout = layout;

            if (layout.Compact)
            {
                var button = CreateButton(Cells[PrimaryIndex], ShowPicker);
                button.ToolTip = (DataContext as ToolItemSettings)?.DisplayName;
                Content = button;
                return;
            }

            var grid = new UniformGrid { Columns = layout.Columns };
            foreach (var cell in Cells)
                grid.Children.Add(CreateButton(cell, () => Run(cell.CommandName)));

            Content = grid;
        }

        private static Button CreateButton(CommandPadCell cell, Action onClick)
        {
            var button = new Button
            {
                Content = new Viewbox { Child = cell.CreateGlyph(), Stretch = Stretch.Uniform },
                Padding = new Thickness(2),
                ToolTip = cell.ToolTip,
            };
            button.SetResourceReference(ForegroundProperty, YMM4Colors.IconBrushKey);
            button.Click += (_, e) =>
            {
                e.Handled = true;
                onClick();
            };
            return button;
        }

        private void ShowPicker() => Dispatcher.BeginInvoke(OpenPicker, System.Windows.Threading.DispatcherPriority.Input);

        private void OpenPicker()
        {
            var menu = new ContextMenu { PlacementTarget = this, Placement = PlacementMode.Bottom };

            foreach (var cell in Cells)
                menu.Items.Add(CreateMenuItem(cell.ToolTip, cell.CommandName));

            var extras = ExtraMenuItems.ToList();
            if (extras.Count > 0)
            {
                menu.Items.Add(new Separator());
                foreach (var (commandName, header) in extras)
                    menu.Items.Add(CreateMenuItem(header, commandName));
            }

            menu.IsOpen = true;
        }

        private MenuItem CreateMenuItem(string header, string commandName)
        {
            var item = new MenuItem { Header = header, IsEnabled = FindCommand(commandName)?.CanExecute(null) == true };
            item.Click += (_, _) => Run(commandName);
            return item;
        }

        private void Run(string commandName)
        {
            if (FindCommand(commandName) is { } command && command.CanExecute(null))
                command.Execute(null);
        }

        private ICommand? FindCommand(string commandName) => FindToolBox()?.Commands.GetValueOrDefault(commandName);

        public static Geometry Glyph(string data)
        {
            var geometry = Geometry.Parse(data);
            geometry.Freeze();
            return geometry;
        }
    }
}
