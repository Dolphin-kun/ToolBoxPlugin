using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ToolBox.Controls
{
    /// <summary>
    /// 内側のリストがホイールを独り占めして、外側の設定画面がスクロールできなくなるのを防ぐ。
    /// 内側でまだスクロールできるときだけそちらに任せ、端まで来たら外側へ流す。
    /// </summary>
    public static class ScrollBubbling
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ScrollBubbling),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element)
                return;

            element.PreviewMouseWheel -= OnPreviewMouseWheel;

            if (e.NewValue is true)
                element.PreviewMouseWheel += OnPreviewMouseWheel;
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not UIElement element || e.Handled)
                return;

            if (CanScrollFurther(element, e.Delta))
                return;

            e.Handled = true;

            var forwarded = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender,
            };

            (VisualTreeHelper.GetParent(element) as UIElement)?.RaiseEvent(forwarded);
        }

        private static bool CanScrollFurther(DependencyObject element, int delta)
        {
            var scrollViewer = FindScrollViewer(element);
            if (scrollViewer == null || scrollViewer.ScrollableHeight <= 0)
                return false;

            return delta < 0
                ? scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight
                : scrollViewer.VerticalOffset > 0;
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject parent)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is ScrollViewer scrollViewer)
                    return scrollViewer;

                if (FindScrollViewer(child) is { } found)
                    return found;
            }

            return null;
        }
    }
}
