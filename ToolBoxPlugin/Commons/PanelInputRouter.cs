using System.Windows;
using System.Windows.Input;

namespace ToolBox.Commons
{
    public static class PanelInputRouter
    {
        private static readonly List<Func<HotkeyTrigger, bool>> handlers = [];
        private static bool isInstalled;

        public static void Add(Func<HotkeyTrigger, bool> handler)
        {
            EnsureInstalled();
            handlers.Add(handler);
        }

        public static void AddFirst(Func<HotkeyTrigger, bool> handler)
        {
            EnsureInstalled();
            handlers.Insert(0, handler);
        }

        public static void Remove(Func<HotkeyTrigger, bool> handler) => handlers.Remove(handler);

        private static void EnsureInstalled()
        {
            if (isInstalled)
                return;

            isInstalled = true;

            EventManager.RegisterClassHandler(
                typeof(Window),
                UIElement.PreviewKeyDownEvent,
                new KeyEventHandler(OnPreviewKeyDown),
                handledEventsToo: true);

            EventManager.RegisterClassHandler(
                typeof(Window),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler(OnPreviewMouseDown),
                handledEventsToo: true);
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            if (Dispatch(HotkeyTrigger.FromKey(key, Keyboard.Modifiers)))
                e.Handled = true;
        }

        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var trigger = HotkeyTrigger.FromMouse(e.ChangedButton, Keyboard.Modifiers);

            if (!HotkeyGesture.IsAssignable(trigger, HotkeyScope.PanelItem))
                return;

            if (Dispatch(trigger))
                e.Handled = true;
        }

        private static bool Dispatch(HotkeyTrigger trigger)
        {
            foreach (var handler in handlers.ToList())
            {
                if (handler(trigger))
                    return true;
            }

            return false;
        }
    }
}
