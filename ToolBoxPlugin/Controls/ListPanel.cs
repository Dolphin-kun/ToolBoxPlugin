using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Controls
{
    public sealed class ListPanel : DockPanel
    {
        private const double RowHeight = 25;
        private const double ButtonColumnWidth = 30;
        private const double IconSize = 14;

        private readonly UniformGrid buttons = new() { Columns = 1 };
        private readonly List<Button> bottomButtons = [];
        private ItemsControl list;

        public ItemsControl List
        {
            get => list;
            set
            {
                Children.Remove(list);
                list = value;
                Prepare(list);
                Children.Add(list);
            }
        }

        public ListBox? ListBox => list as ListBox;

        public ListPanel()
        {
            var sizes = EffectEditorSettings.Default.VideoEditorSize;
            SetBinding(HeightProperty, new Binding(nameof(sizes.ListHeight)) { Source = sizes, Mode = BindingMode.OneWay });

            var grip = new Thumb { Height = 4, Cursor = Cursors.SizeNS, Opacity = 0 };
            grip.DragDelta += (_, e) => sizes.ListHeight += e.VerticalChange;
            SetDock(grip, Dock.Bottom);
            Children.Add(grip);

            buttons.Width = ButtonColumnWidth;
            SetDock(buttons, Dock.Right);
            Children.Add(buttons);

            list = new ListBox();
            Prepare(list);
            Children.Add(list);
        }

        private void Prepare(ItemsControl control)
        {
            control.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            control.PreviewMouseWheel += OnPreviewMouseWheel;

            if (control is not ListBox listBox)
                return;

            listBox.Loaded += (_, _) =>
            {
                var style = new Style(typeof(ListBoxItem), listBox.TryFindResource(typeof(ListBoxItem)) as Style);
                style.Setters.Add(new Setter(HeightProperty, RowHeight));
                style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
                foreach (var setter in ItemContainerSetters)
                    style.Setters.Add(setter);
                listBox.ItemContainerStyle = style;
            };
        }

        public List<SetterBase> ItemContainerSetters { get; } = [];

        public Button AddButton(object geometryKey, string tip, Action onClick, bool bottom = false)
        {
            var button = ColumnButton(geometryKey, tip);
            button.Click += (_, e) =>
            {
                e.Handled = true;
                onClick();
            };
            Insert(button, bottom);
            return button;
        }

        public Button AddCommandButton(object geometryKey, string tip, ICommand command, bool passSelectedItem = false)
        {
            var button = ColumnButton(geometryKey, tip);
            button.Command = command;
            if (passSelectedItem)
                button.SetBinding(ButtonBase.CommandParameterProperty, new Binding(nameof(Selector.SelectedItem)) { Source = list });

            Insert(button, false);
            return button;
        }

        public Button AddMenuButton(object geometryKey, string tip, Action<ContextMenu> fill, bool bottom = false)
        {
            var button = ColumnButton(geometryKey, tip);
            var menu = new ContextMenu();
            button.Click += (_, e) =>
            {
                e.Handled = true;
                Dispatcher.BeginInvoke(() =>
                {
                    menu.Items.Clear();
                    fill(menu);
                    menu.PlacementTarget = button;
                    menu.Placement = PlacementMode.Bottom;
                    menu.IsOpen = true;
                }, DispatcherPriority.Input);
            };
            Insert(button, bottom);
            return button;
        }

        public void ClearButtons()
        {
            buttons.Children.Clear();
            bottomButtons.Clear();
        }

        public static MenuItem MenuItem(string header, Action onClick, bool enabled = true, string? toolTip = null)
        {
            var item = new MenuItem { Header = header, IsEnabled = enabled, ToolTip = toolTip };
            item.Click += (_, _) => onClick();
            return item;
        }

        private void Insert(Button button, bool bottom)
        {
            if (bottom)
            {
                bottomButtons.Add(button);
                buttons.Children.Add(button);
                return;
            }

            buttons.Children.Insert(buttons.Children.Count - bottomButtons.Count, button);
        }

        private static Button ColumnButton(object geometryKey, string tip)
        {
            var icon = new System.Windows.Shapes.Path { Stretch = Stretch.Uniform, MaxWidth = IconSize, MaxHeight = IconSize };
            icon.SetResourceReference(System.Windows.Shapes.Path.DataProperty, geometryKey);
            icon.SetBinding(System.Windows.Shapes.Shape.FillProperty, new Binding(nameof(Control.Foreground))
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(Control) },
            });

            var button = new Button { Content = icon, Padding = new Thickness(4), ToolTip = tip };
            button.IsEnabledChanged += (_, _) => icon.Opacity = button.IsEnabled ? 1 : 0.35;
            return button;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewer = FindScrollViewer(list);
            var canScroll = viewer is not null && (e.Delta > 0 ? viewer.VerticalOffset > 0 : viewer.VerticalOffset < viewer.ScrollableHeight);
            if (canScroll)
                return;

            e.Handled = true;
            RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = MouseWheelEvent, Source = this });
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject parent)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer viewer)
                    return viewer;
                if (FindScrollViewer(child) is { } found)
                    return found;
            }

            return null;
        }
    }
}
