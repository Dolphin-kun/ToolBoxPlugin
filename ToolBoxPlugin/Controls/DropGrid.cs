using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ToolBox.Controls
{
    public class DropGrid : Grid
    {
        private Window? parentWindow;

        public event EventHandler? GridChanged;
        public event EventHandler<HighlightEventArgs>? HighlightRequested;
        public event EventHandler? HighlightCleared;

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

        public static readonly DependencyProperty HighlightPaddingProperty =
            DependencyProperty.Register(nameof(HighlightPadding), typeof(double), typeof(DropGrid),
                new PropertyMetadata(2.0));

        public static readonly DependencyPropertyKey IsCtrlPressedPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsCtrlPressed), typeof(bool), typeof(DropGrid),
                new PropertyMetadata(false, OnGridPropertyChanged));

        public static readonly DependencyProperty IsCtrlPressedProperty = IsCtrlPressedPropertyKey.DependencyProperty;

        public int RowCount { get => (int)GetValue(RowCountProperty); set => SetValue(RowCountProperty, value); }
        public int ColumnCount { get => (int)GetValue(ColumnCountProperty); set => SetValue(ColumnCountProperty, value); }
        public double CellSize { get => (double)GetValue(CellSizeProperty); set => SetValue(CellSizeProperty, value); }
        public double CellMargin { get => (double)GetValue(CellMarginProperty); set => SetValue(CellMarginProperty, value); }
        public Brush CellBorderBrush { get => (Brush)GetValue(CellBorderBrushProperty); set => SetValue(CellBorderBrushProperty, value); }
        public bool ShowHighlight { get => (bool)GetValue(ShowHighlightProperty); set => SetValue(ShowHighlightProperty, value); }
        public Color HighlightColor { get => (Color)GetValue(HighlightColorProperty); set => SetValue(HighlightColorProperty, value); }
        public bool ShowCellBorders { get => (bool)GetValue(ShowCellBordersProperty); set => SetValue(ShowCellBordersProperty, value); }
        public double HighlightPadding { get => (double)GetValue(HighlightPaddingProperty); set => SetValue(HighlightPaddingProperty, value); }
        public bool IsCtrlPressed => (bool)GetValue(IsCtrlPressedProperty);

        public DropGrid()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public void HighlightCell(int row, int col, Size cellSpan)
        {
            if (!ShowHighlight)
            {
                ClearHighlight();
                return;
            }

            int spanX = (int)cellSpan.Width;
            int spanY = (int)cellSpan.Height;

            int startRow = Math.Max(0, Math.Min(row, RowCount - spanY));
            int startCol = Math.Max(0, Math.Min(col, ColumnCount - spanX));
            int rowSpan = Math.Min(spanY, RowCount - startRow);
            int colSpan = Math.Min(spanX, ColumnCount - startCol);

            HighlightRequested?.Invoke(this, new HighlightEventArgs(startRow, startCol, rowSpan, colSpan));
        }

        public void ClearHighlight() => HighlightCleared?.Invoke(this, EventArgs.Empty);

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

        private static void OnGridPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DropGrid grid && grid.IsLoaded)
                grid.UpdateGrid();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(this);
            AttachWindowKeyHandlers();
            UpdateGrid();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachWindowKeyHandlers();
        }

        private void AttachWindowKeyHandlers()
        {
            if (parentWindow == null) return;
            DetachWindowKeyHandlers();
            parentWindow.PreviewKeyDown += OnParentWindowKeyChanged;
            parentWindow.PreviewKeyUp += OnParentWindowKeyChanged;
            parentWindow.PreviewMouseMove += OnParentWindowMouseMove;
            parentWindow.Deactivated += OnParentWindowDeactivated;
        }

        private void DetachWindowKeyHandlers()
        {
            if (parentWindow == null) return;
            parentWindow.PreviewKeyDown -= OnParentWindowKeyChanged;
            parentWindow.PreviewKeyUp -= OnParentWindowKeyChanged;
            parentWindow.PreviewMouseMove -= OnParentWindowMouseMove;
            parentWindow.Deactivated -= OnParentWindowDeactivated;
        }

        private void OnParentWindowKeyChanged(object sender, KeyEventArgs e) => UpdateCtrlState();

        /// <summary>
        /// コンテキストメニューは別ウィンドウなので、開いている間に Ctrl を離してもキーイベントが届かない。
        /// メニューを閉じたあとのマウス操作で取りこぼしを拾い直す。
        /// </summary>
        private void OnParentWindowMouseMove(object sender, MouseEventArgs e) => UpdateCtrlState();

        private void OnParentWindowDeactivated(object? sender, EventArgs e) => SetCtrlPressed(false);

        /// <summary>Ctrl の押下状態を今の実際のキー状態に合わせ直す。</summary>
        public void RefreshCtrlState() => UpdateCtrlState();

        private void UpdateCtrlState() => SetCtrlPressed(
            (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

        private void SetCtrlPressed(bool pressed)
        {
            if (IsCtrlPressed == pressed) return;
            SetValue(IsCtrlPressedPropertyKey, pressed);
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

            if (ShowCellBorders || IsCtrlPressed)
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

            GridChanged?.Invoke(this, EventArgs.Empty);
        }

        public sealed class HighlightEventArgs(int row, int col, int rowSpan, int colSpan) : EventArgs
        {
            public int Row { get; } = row;
            public int Col { get; } = col;
            public int RowSpan { get; } = rowSpan;
            public int ColSpan { get; } = colSpan;
        }
    }
}
