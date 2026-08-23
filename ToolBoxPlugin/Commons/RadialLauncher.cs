using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ToolBox.Controls;
using ToolBox.Settings;
using YukkuriMovieMaker.Views;

namespace ToolBox.Commons
{
    public static class RadialLauncher
    {
        private static bool isInstalled;

        private static Window? window;
        private static Popup? popup;
        private static Point center;

        private static readonly List<IReadOnlyList<RadialEntry>> path = [];

        private static MouseButton? openedWith;
        private static bool skipNextButtonUp;

        private static bool IsOpen => popup != null;

        public static void Install()
        {
            if (isInstalled)
                return;

            isInstalled = true;

            if (Application.Current is { } app && !app.Dispatcher.CheckAccess())
            {
                app.Dispatcher.BeginInvoke(RegisterHandlers);
                return;
            }

            RegisterHandlers();
        }

        private static void RegisterHandlers()
        {
            EventManager.RegisterClassHandler(
                typeof(Window),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler(OnMouseDown),
                handledEventsToo: true);

            EventManager.RegisterClassHandler(
                typeof(Window),
                UIElement.PreviewMouseUpEvent,
                new MouseButtonEventHandler(OnMouseUp),
                handledEventsToo: true);

            EventManager.RegisterClassHandler(
                typeof(FrameworkElement),
                FrameworkElement.ContextMenuOpeningEvent,
                new ContextMenuEventHandler(OnContextMenuOpening),
                handledEventsToo: true);
        }

        private static HotkeyTrigger? Trigger =>
            HotkeyGesture.ParseAssignable(
                ToolBoxSettings.Default.RadialLauncherGesture,
                HotkeyScope.RadialLauncher);

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsOpen)
            {
                e.Handled = true;

                if (e.ChangedButton == MouseButton.Left)
                    Close();

                return;
            }

            if (sender is not Window sourceWindow)
                return;

            if (!ToolBoxSettings.Default.IsRadialLauncherEnabled)
                return;

            if (Trigger is not { } trigger)
                return;

            if (HotkeyTrigger.FromMouse(e.ChangedButton, Keyboard.Modifiers) != trigger)
                return;

            if (!IsOnTimeline(e.OriginalSource))
                return;

            var entries = RadialEntryBuilder.BuildRoot();
            if (entries.Count == 0)
                return;

            e.Handled = true;

            window = sourceWindow;
            center = e.GetPosition(sourceWindow);
            openedWith = e.ChangedButton;

            Open(entries);
        }

        private static void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsOpen || e.ChangedButton != openedWith)
                return;

            e.Handled = true;

            if (skipNextButtonUp)
            {
                skipNextButtonUp = false;
                return;
            }

            GoBack();
        }

        private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (IsOpen)
                e.Handled = true;
        }

        private static void Open(IReadOnlyList<RadialEntry> entries)
        {
            if (window == null || IsOpen)
                return;

            skipNextButtonUp = true;

            path.Clear();
            path.Add(entries);

            popup = new Popup
            {
                Placement = PlacementMode.Relative,
                PlacementTarget = window,
                HorizontalOffset = center.X - RadialMenu.OuterRadius,
                VerticalOffset = center.Y - RadialMenu.OuterRadius,
                AllowsTransparency = true,
                StaysOpen = true,
            };

            ShowCurrentLevel();
            popup.IsOpen = true;

            window.PreviewKeyDown += OnKeyDown;

            window.Deactivated += OnWindowLeft;
            window.StateChanged += OnWindowLeft;
            window.LocationChanged += OnWindowLeft;
        }

        private static void OnWindowLeft(object? sender, EventArgs e) => Close();

        private static void ShowCurrentLevel()
        {
            if (popup == null || path.Count == 0)
                return;

            var menu = new RadialMenu(path[^1], hasParent: path.Count > 1);
            menu.EntryChosen += OnEntryChosen;
            menu.BackRequested += GoBack;

            popup.Child = menu;
        }

        private static void OnEntryChosen(RadialEntry entry)
        {
            if (entry.IsFolder)
            {
                path.Add(entry.Children);
                ShowCurrentLevel();
                return;
            }

            var invoke = entry.Invoke;
            Close();
            invoke?.Invoke();
        }

        private static void GoBack()
        {
            if (path.Count <= 1)
            {
                Close();
                return;
            }

            path.RemoveAt(path.Count - 1);
            ShowCurrentLevel();
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            e.Handled = true;
            GoBack();
        }

        private static void Close()
        {
            skipNextButtonUp = false;
            openedWith = null;
            path.Clear();

            if (window != null)
            {
                window.PreviewKeyDown -= OnKeyDown;
                window.Deactivated -= OnWindowLeft;
                window.StateChanged -= OnWindowLeft;
                window.LocationChanged -= OnWindowLeft;
            }

            if (popup != null)
            {
                popup.IsOpen = false;
                popup.Child = null;
            }

            popup = null;
            window = null;
        }

        private static bool IsOnTimeline(object? source)
        {
            var current = source as DependencyObject;

            while (current != null)
            {
                if (current is TimelineView)
                    return true;

                current = current is Visual or System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(current)
                    : (current as FrameworkContentElement)?.Parent;
            }

            return false;
        }
    }
}
