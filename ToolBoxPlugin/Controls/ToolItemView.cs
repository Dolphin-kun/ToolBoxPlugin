using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ToolBox.Commons;
using ToolBox.ViewModel;

namespace ToolBox.Controls
{
    public class ToolItemView : UserControl
    {
        public static readonly DependencyProperty CellSpanProperty =
            DependencyProperty.RegisterAttached("CellSpan", typeof(Size), typeof(ToolItemView),
                new FrameworkPropertyMetadata(new Size(1, 1), FrameworkPropertyMetadataOptions.Inherits, OnCellSpanChanged));

        public static Size GetCellSpan(DependencyObject element) => (Size)element.GetValue(CellSpanProperty);

        public static void SetCellSpan(DependencyObject element, Size value) => element.SetValue(CellSpanProperty, value);

        public ToolItemView()
        {
            Background = Brushes.Transparent;
        }

        public Size CellSpan => GetCellSpan(this);

        public bool IsCompact => CellSpan.Width < 2 && CellSpan.Height < 2;

        private static void OnCellSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToolItemView view)
                view.OnCellSpanChanged();
        }

        protected virtual void OnCellSpanChanged()
        {
        }

        public virtual FrameworkElement? CreateSettingsPanel() => ItemSettingsPanel.Create(DataContext);

        protected ToolBoxViewModel? FindToolBox()
        {
            DependencyObject? current = this;
            while (current != null)
            {
                if (current is FrameworkElement { DataContext: ToolBoxViewModel viewModel })
                    return viewModel;

                current = VisualTreeHelper.GetParent(current);
            }

            return ToolBoxViewModel.Instance;
        }
    }
}
