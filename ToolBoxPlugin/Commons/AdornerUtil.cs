using System.Windows;
using System.Windows.Documents;
using ToolBox.Controls;

namespace ToolBox.Commons
{
    public class AdornerUtil
    {
        private SettingsAdorner? _adorner;
        private UIElement? _owner;

        public void Show<TPanel>(UIElement owner, Func<TPanel> panelFactory) where TPanel : FrameworkElement
        {
            ArgumentNullException.ThrowIfNull(panelFactory);

            _owner = owner;

            var adornerLayer = AdornerLayer.GetAdornerLayer(owner);
            if (adornerLayer == null) return;

            var panel = panelFactory();
            _adorner = new SettingsAdorner(owner, panel);
            _adorner.DismissRequested += (_, _) => Hide();

            adornerLayer.Add(_adorner);
            
            var window = Window.GetWindow(owner);
            if (window != null)
                _adorner.AttachDismissHandler(window);
        }

        public void Hide()
        {
            if (_adorner == null || _owner == null) return;

            _adorner.DetachDismissHandler();
            AdornerLayer.GetAdornerLayer(_owner)?.Remove(_adorner);
            _adorner = null;
            _owner = null;
        }

        public void Toggle<TPanel>(UIElement owner, Func<TPanel> panelFactory) where TPanel : FrameworkElement
        {
            if (_adorner != null)
                Hide();
            else
                Show(owner, panelFactory);
        }

        public bool IsVisible => _adorner != null;
    }
}
