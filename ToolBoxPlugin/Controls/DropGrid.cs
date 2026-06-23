using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ToolBox.Controls
{
    public class DropGrid : Grid
    {
        private Border? _highlightBorder;

        public event EventHandler? GridChanged;

        public static readonly DependencyProperty RowCountProperty =
            DependencyProperty.Register(nameof(RowCount), typeof(int), typeof(DropGrid),
                new PropertyMetadata(4, OnGridPropertyChanged));

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(DropGrid),
                new PropertyMetadata(3, OnGridPropertyChanged));

        public static readonly DependencyProperty CellSizeProperty =
            DependencyProperty.Register(nameof(CellSize), typeof(double), typeof(DropGrid),
                new PropertyMetadata(60.0, OnGridPropertyChanged));

        public static readonly DependencyProperty CellMarginProperty =
            DependencyProperty.Register(nameof(CellMargin), typeof(double), typeof(DropGrid),
                new PropertyMetadata(2.0, OnGridPropertyChanged));

        public static readonly DependencyProperty CellBorderBrushProperty =
            DependencyProperty.Register(nameof(CellBorderBrush), typeof(Brush), typeof(DropGrid),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)), OnGridPropertyChanged));

        public static readonly DependencyProperty ShowHighlightProperty =
            DependencyProperty.Register(nameof(ShowHighlight), typeof(bool), typeof(DropGrid),
                new PropertyMetadata(true, OnGridPropertyChanged));

        public static readonly DependencyProperty HighlightColorProperty =
            DependencyProperty.Register(nameof(HighlightColor), typeof(Color), typeof(DropGrid),
                new PropertyMetadata(Color.FromArgb(0xFF, 0x00, 0x99, 0xFF), OnGridPropertyChanged));

        public static readonly DependencyProperty ShowCellBordersProperty =
            DependencyProperty.Register(nameof(ShowCellBorders), typeof(bool), typeof(DropGrid),
                new PropertyMetadata(true, OnGridPropertyChanged));

        public int RowCount { get => (int)GetValue(RowCountProperty); set => SetValue(RowCountProperty, value); }
        public int ColumnCount { get => (int)GetValue(ColumnCountProperty); set => SetValue(ColumnCountProperty, value); }
        public double CellSize { get => (double)GetValue(CellSizeProperty); set => SetValue(CellSizeProperty, value); }
        public double CellMargin { get => (double)GetValue(CellMarginProperty); set => SetValue(CellMarginProperty, value); }
        public Brush CellBorderBrush { get => (Brush)GetValue(CellBorderBrushProperty); set => SetValue(CellBorderBrushProperty, value); }
        public bool ShowHighlight { get => (bool)GetValue(ShowHighlightProperty); set => SetValue(ShowHighlightProperty, value); }
        public Color HighlightColor { get => (Color)GetValue(HighlightColorProperty); set => SetValue(HighlightColorProperty, value); }
        public bool ShowCellBorders { get => (bool)GetValue(ShowCellBordersProperty); set => SetValue(ShowCellBordersProperty, value); }

        private Window? _parentWindow;
        private bool _isCtrlPressed;

        public DropGrid()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            AttachWindowKeyHandlers();
            UpdateGrid();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachWindowKeyHandlers();
        }

        private void AttachWindowKeyHandlers()
        {
            if (_parentWindow == null) return;
            _parentWindow.PreviewKeyDown -= OnParentWindowKeyChanged;
            _parentWindow.PreviewKeyUp -= OnParentWindowKeyChanged;
            _parentWindow.Deactivated -= OnParentWindowDeactivated;
            _parentWindow.PreviewKeyDown += OnParentWindowKeyChanged;
            _parentWindow.PreviewKeyUp += OnParentWindowKeyChanged;
            _parentWindow.Deactivated += OnParentWindowDeactivated;
        }

        private void DetachWindowKeyHandlers()
        {
            if (_parentWindow == null) return;
            _parentWindow.PreviewKeyDown -= OnParentWindowKeyChanged;
            _parentWindow.PreviewKeyUp -= OnParentWindowKeyChanged;
            _parentWindow.Deactivated -= OnParentWindowDeactivated;
        }

        private void OnParentWindowKeyChanged(object sender, KeyEventArgs e)
        {
            bool ctrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (_isCtrlPressed != ctrlPressed)
            {
                _isCtrlPressed = ctrlPressed;
                UpdateGrid();
            }
        }

        private void OnParentWindowDeactivated(object? sender, EventArgs e)
        {
            if (_isCtrlPressed)
            {
                _isCtrlPressed = false;
                UpdateGrid();
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            UpdateCtrlState();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateCtrlState();
        }

        private void UpdateCtrlState()
        {
            bool ctrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (_isCtrlPressed != ctrlPressed)
            {
                _isCtrlPressed = ctrlPressed;
                UpdateGrid();
            }
        }

        private static void OnGridPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DropGrid grid && grid.IsLoaded)
                grid.UpdateGrid();
        }

        private void UpdateGrid()
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
            Children.Clear();

            for (int i = 0; i < RowCount; i++)
                RowDefinitions.Add(new RowDefinition { Height = new GridLength(CellSize) });

            for (int i = 0; i < ColumnCount; i++)
                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(CellSize) });

            if (ShowCellBorders || _isCtrlPressed)
            {
                for (int r = 0; r < RowCount; r++)
                {
                    for (int c = 0; c < ColumnCount; c++)
                    {
                        var cellBorder = new Border
                        {
                            BorderBrush = CellBorderBrush,
                            BorderThickness = new Thickness(1),
                            Margin = new Thickness(CellMargin),
                            SnapsToDevicePixels = true,
                            IsHitTestVisible = false
                        };
                        SetRow(cellBorder, r);
                        SetColumn(cellBorder, c);
                        Children.Add(cellBorder);
                    }
                }
            }

            if (_highlightBorder != null)
            {
                Children.Add(_highlightBorder);
            }

            GridChanged?.Invoke(this, EventArgs.Empty);
        }

        public static readonly DependencyProperty HighlightPaddingProperty =
            DependencyProperty.Register(nameof(HighlightPadding), typeof(double), typeof(DropGrid),
                new PropertyMetadata(2.0));

        public double HighlightPadding
        {
            get => (double)GetValue(HighlightPaddingProperty);
            set => SetValue(HighlightPaddingProperty, value);
        }

        public void HighlightCell(int row, int col, Size cellSpan)
        {
            ClearHighlight();

            if (!ShowHighlight) return;

            int spanX = (int)cellSpan.Width;
            int spanY = (int)cellSpan.Height;

            int startRow = Math.Max(0, Math.Min(row, RowCount - spanY));
            int startCol = Math.Max(0, Math.Min(col, ColumnCount - spanX));

            double margin = Math.Max(0, CellMargin - HighlightPadding);

            var brushColor = HighlightColor;
            var borderBrush = new SolidColorBrush(brushColor);
            var bgBrush = new SolidColorBrush(Color.FromArgb(0x30, brushColor.R, brushColor.G, brushColor.B));

            _highlightBorder = new Border
            {
                Background = bgBrush,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1.5),
                Margin = new Thickness(margin),
                IsHitTestVisible = false
            };

            SetRow(_highlightBorder, startRow);
            SetColumn(_highlightBorder, startCol);
            SetRowSpan(_highlightBorder, Math.Min(spanY, RowCount - startRow));
            SetColumnSpan(_highlightBorder, Math.Min(spanX, ColumnCount - startCol));

            Children.Add(_highlightBorder);
        }

        public void ClearHighlight()
        {
            if (_highlightBorder != null)
            {
                Children.Remove(_highlightBorder);
                _highlightBorder = null;
            }
        }

        public (int row, int col)? GetCellPosition(Point position)
        {
            int col = (int)(position.X / CellSize);
            int row = (int)(position.Y / CellSize);

            if (row < 0 || row >= RowCount || col < 0 || col >= ColumnCount)
                return null;

            return (row, col);
        }

        public Point GetCellTopLeft(int row, int col)
            => new(col * CellSize, row * CellSize);
    }
}
