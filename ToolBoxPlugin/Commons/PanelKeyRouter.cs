using System.Windows;
using System.Windows.Input;

namespace ToolBox.Commons
{
    public static class PanelKeyRouter
    {
        private static readonly List<Func<Key, ModifierKeys, bool>> handlers = [];
        private static bool isInstalled;

        public static void Add(Func<Key, ModifierKeys, bool> handler)
        {
            EnsureInstalled();
            handlers.Add(handler);
        }

        /// <summary>
        /// 他のどのハンドラーよりも先にキーを受け取る。
        /// ホットキーを設定している最中に、そのキーが割り当て済みの操作を起動しないようにするために使う。
        /// </summary>
        public static void AddFirst(Func<Key, ModifierKeys, bool> handler)
        {
            EnsureInstalled();
            handlers.Insert(0, handler);
        }

        public static void Remove(Func<Key, ModifierKeys, bool> handler) => handlers.Remove(handler);

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
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            foreach (var handler in handlers.ToList())
            {
                if (!handler(key, Keyboard.Modifiers))
                    continue;

                e.Handled = true;
                return;
            }
        }
    }
}
