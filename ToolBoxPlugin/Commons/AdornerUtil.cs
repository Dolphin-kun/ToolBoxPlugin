using System.Windows;
using System.Windows.Documents;
using ToolBox.Controls;

namespace ToolBox.Commons
{
    public class AdornerUtil
    {
        private SettingsAdorner? adorner;
        private UIElement? owner;

        public bool IsVisible => adorner != null;

        public void Show<TPanel>(UIElement owner, Func<TPanel> panelFactory) where TPanel : FrameworkElement
        {
            ArgumentNullException.ThrowIfNull(panelFactory);

            this.owner = owner;

            var adornerLayer = AdornerLayer.GetAdornerLayer(owner);
            if (adornerLayer == null) return;

            var panel = panelFactory();
            adorner = new SettingsAdorner(owner, panel);
            adorner.DismissRequested += (_, _) => Hide();

            adornerLayer.Add(adorner);
            
            var window = Window.GetWindow(owner);
            if (window != null)
                adorner.AttachDismissHandler(window);
        }

        public void Hide()
        {
            if (adorner == null || owner == null) return;

            adorner.DetachDismissHandler();
            AdornerLayer.GetAdornerLayer(owner)?.Remove(adorner);
            adorner = null;
            owner = null;
        }

        public void Toggle<TPanel>(UIElement owner, Func<TPanel> panelFactory) where TPanel : FrameworkElement
        {
            if (adorner != null)
                Hide();
            else
                Show(owner, panelFactory);
        }
    }
}
