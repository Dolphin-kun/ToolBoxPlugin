using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ToolBox.Controls
{
    public class SettingsAdorner : Adorner
    {
        private readonly VisualCollection visuals;
        private readonly ContentPresenter presenter;
        private UIElement? dismissOwner;

        public event EventHandler? DismissRequested;

        public SettingsAdorner(UIElement adornedElement, UIElement content) : base(adornedElement)
        {
            visuals = new VisualCollection(this);
            presenter = new ContentPresenter { Content = content };
            visuals.Add(presenter);
            IsHitTestVisible = true;
        }

        protected override int VisualChildrenCount => visuals.Count;
        protected override Visual GetVisualChild(int index) => visuals[index];

        protected override Size MeasureOverride(Size constraint)
        {
            presenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            return new Size(
                Math.Max(AdornedElement.RenderSize.Width, presenter.DesiredSize.Width),
                Math.Max(AdornedElement.RenderSize.Height, presenter.DesiredSize.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            presenter.Arrange(new Rect(new Point(0, 0), presenter.DesiredSize));
            
            return new Size(
                Math.Max(finalSize.Width, presenter.DesiredSize.Width),
                Math.Max(finalSize.Height, presenter.DesiredSize.Height));
        }

        public void AttachDismissHandler(UIElement owner)
        {
            dismissOwner = owner;
            owner.AddHandler(Mouse.PreviewMouseDownEvent,
                new MouseButtonEventHandler(OnOwnerPreviewMouseDown), handledEventsToo: false);
        }

        public void DetachDismissHandler()
        {
            dismissOwner?.RemoveHandler(Mouse.PreviewMouseDownEvent,
                new MouseButtonEventHandler(OnOwnerPreviewMouseDown));
            dismissOwner = null;
        }

        private void OnOwnerPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsAnyPopupOpen(presenter))
                return;

            var point = e.GetPosition(presenter);
            
            var rect = new Rect(0, 0, presenter.RenderSize.Width, presenter.RenderSize.Height);
            if (!rect.Contains(point))
            {
                DismissRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private static bool IsAnyPopupOpen(DependencyObject parent)
        {
            if (parent == null) return false;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is ComboBox comboBox && comboBox.IsDropDownOpen)
                    return true;

                if (child is System.Windows.Controls.Primitives.Popup popup && popup.IsOpen)
                    return true;

                if (IsAnyPopupOpen(child))
                    return true;
            }

            return false;
        }
    }
}
