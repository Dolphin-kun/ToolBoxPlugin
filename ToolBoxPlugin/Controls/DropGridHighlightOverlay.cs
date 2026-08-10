using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToolBox.Controls
{
    public class DropGridHighlightOverlay : Canvas
    {
        private Border? highlightBorder;

        public static readonly DependencyProperty DropGridProperty =
            DependencyProperty.Register(nameof(DropGrid), typeof(DropGrid), typeof(DropGridHighlightOverlay),
                new PropertyMetadata(null, OnDropGridChanged));

        public DropGrid? DropGrid
        {
            get => (DropGrid?)GetValue(DropGridProperty);
            set => SetValue(DropGridProperty, value);
        }

        private static void OnDropGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DropGridHighlightOverlay overlay) return;

            if (e.OldValue is DropGrid oldGrid)
            {
                oldGrid.HighlightRequested -= overlay.OnHighlightRequested;
                oldGrid.HighlightCleared -= overlay.OnHighlightCleared;
            }

            if (e.NewValue is DropGrid newGrid)
            {
                newGrid.HighlightRequested += overlay.OnHighlightRequested;
                newGrid.HighlightCleared += overlay.OnHighlightCleared;
            }

            overlay.RemoveHighlight();
        }

        private void OnHighlightRequested(object? sender, DropGrid.HighlightEventArgs e)
        {
            if (DropGrid == null) return;

            RemoveHighlight();

            double margin = Math.Max(0, DropGrid.CellMargin - DropGrid.HighlightPadding);
            var color = DropGrid.HighlightColor;

            highlightBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x30, color.R, color.G, color.B)),
                BorderBrush = new SolidColorBrush(color),
                BorderThickness = new Thickness(1.5),
                IsHitTestVisible = false,
                Width = Math.Max(0, e.ColSpan * DropGrid.CellSize - margin * 2),
                Height = Math.Max(0, e.RowSpan * DropGrid.CellSize - margin * 2),
            };

            var topLeft = DropGrid.GetCellTopLeft(e.Row, e.Col);
            SetLeft(highlightBorder, topLeft.X + margin);
            SetTop(highlightBorder, topLeft.Y + margin);

            Children.Add(highlightBorder);
        }

        private void OnHighlightCleared(object? sender, EventArgs e) => RemoveHighlight();

        private void RemoveHighlight()
        {
            if (highlightBorder != null)
            {
                Children.Remove(highlightBorder);
                highlightBorder = null;
            }
        }
    }
}
