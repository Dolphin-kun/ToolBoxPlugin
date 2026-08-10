using System.Windows;

namespace ToolBox.Commons
{
    public static class UiThread
    {
        public static void Run(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(action);
                return;
            }

            action();
        }

        public static void Post(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null)
                dispatcher.BeginInvoke(action);
            else
                action();
        }
    }
}
