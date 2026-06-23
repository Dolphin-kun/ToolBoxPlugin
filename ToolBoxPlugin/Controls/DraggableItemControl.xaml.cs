using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ToolBox.Commons;
using ToolBox.Items;
using ToolBox.Settings;

namespace ToolBox.Controls
{
    public partial class DraggableItemControl : UserControl
    {
        public event EventHandler? DragStarted;
        public event EventHandler? DragDelta;
        public event EventHandler? DragCompleted;
        public event EventHandler<ResizeEventArgs>? ResizeStarted;
        public event EventHandler<ResizeEventArgs>? ResizeDelta;
        public event EventHandler<ResizeEventArgs>? ResizeCompleted;

        private Point _startMousePoint;
        private Point _startElementPosition;
        private Size _startElementSize;
        private bool _isDragging;
        private bool _isResizing;
        private Canvas? _parentCanvas;
        private Window? _parentWindow;

        public static readonly DependencyProperty CellSpanProperty =
            DependencyProperty.Register(nameof(CellSpan), typeof(Size), typeof(DraggableItemControl),
                new FrameworkPropertyMetadata(new Size(1, 1),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnCellSpanChanged));

        public static readonly DependencyProperty DropGridProperty =
            DependencyProperty.Register(nameof(DropGrid), typeof(DropGrid), typeof(DraggableItemControl),
                new PropertyMetadata(null, OnDropGridChanged));

        public Size CellSpan
        {
            get => (Size)GetValue(CellSpanProperty);
            set => SetValue(CellSpanProperty, value);
        }

        public static readonly DependencyProperty MinCellSpanProperty =
            DependencyProperty.Register(nameof(MinCellSpan), typeof(Size), typeof(DraggableItemControl),
                new PropertyMetadata(new Size(1, 1)));

        public Size MinCellSpan
        {
            get => (Size)GetValue(MinCellSpanProperty);
            set => SetValue(MinCellSpanProperty, value);
        }

        public DropGrid? DropGrid
        {
            get => (DropGrid?)GetValue(DropGridProperty);
            set => SetValue(DropGridProperty, value);
        }

        public DraggableItemControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private static void OnCellSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DraggableItemControl item) item.UpdateSize();
        }

        private static void OnDropGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DraggableItemControl item) item.UpdateSize();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _parentCanvas = VisualTreeHelper.GetParent(this) as Canvas;
            UpdateSize();
            _parentWindow = Window.GetWindow(this);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachWindowKeyHandlers();
        }

        private void UpdateSize()
        {
            if (DropGrid == null) return;
            Width = CellSpan.Width * DropGrid.CellSize;
            Height = CellSpan.Height * DropGrid.CellSize;
        }

        private Canvas? ParentCanvas => _parentCanvas;

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            AttachWindowKeyHandlers();
            UpdateHandleVisibility();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_isDragging && !_isResizing)
            {
                DetachWindowKeyHandlers();
                ResizeHandle.Visibility = Visibility.Collapsed;
            }
        }

        private void AttachWindowKeyHandlers()
        {
            if (_parentWindow == null) return;
            _parentWindow.KeyDown -= OnParentWindowKeyChanged;
            _parentWindow.KeyUp -= OnParentWindowKeyChanged;
            _parentWindow.KeyDown += OnParentWindowKeyChanged;
            _parentWindow.KeyUp += OnParentWindowKeyChanged;
        }

        private void DetachWindowKeyHandlers()
        {
            if (_parentWindow == null) return;
            _parentWindow.KeyDown -= OnParentWindowKeyChanged;
            _parentWindow.KeyUp -= OnParentWindowKeyChanged;
        }

        private void OnParentWindowKeyChanged(object sender, KeyEventArgs e) => UpdateHandleVisibility();

        private void UpdateHandleVisibility()
        {
            if (_isResizing) return;
            bool ctrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (IsMouseOver && ctrlPressed)
            {
                ResizeHandle.Visibility = Visibility.Visible;
                Cursor = Cursors.SizeAll;
            }
            else
            {
                ResizeHandle.Visibility = Visibility.Collapsed;
                Cursor = Cursors.Hand;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            bool ctrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (!ctrlPressed) return;

            if (e.OriginalSource is DependencyObject src && IsAncestorOf(ResizeHandle, src)) return;

            var canvas = ParentCanvas;
            if (canvas == null) return;

            _startMousePoint = e.GetPosition(canvas);
            _startElementPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            if (double.IsNaN(_startElementPosition.X)) _startElementPosition.X = 0;
            if (double.IsNaN(_startElementPosition.Y)) _startElementPosition.Y = 0;

            _isDragging = false;
            Panel.SetZIndex(this, 999);
            CaptureMouse();
            DragStarted?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isResizing) { HandleResizeMove(e); return; }

            var canvas = ParentCanvas;
            if (!IsMouseCaptured || canvas == null || e.LeftButton != MouseButtonState.Pressed) return;

            Point current = e.GetPosition(canvas);

            if (!_isDragging)
            {
                Vector diff = _startMousePoint - current;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                    _isDragging = true;
            }

            if (_isDragging && DropGrid != null)
            {
                double newLeft = _startElementPosition.X + (current.X - _startMousePoint.X);
                double newTop = _startElementPosition.Y + (current.Y - _startMousePoint.Y);

                var (row, col) = PixelToClampedCell(newLeft, newTop);
                var snapped = DropGrid.GetCellTopLeft(row, col);
                Canvas.SetLeft(this, snapped.X);
                Canvas.SetTop(this, snapped.Y);

                DragDelta?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_isResizing) { HandleResizeUp(e); return; }

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                Panel.SetZIndex(this, 0);

                if (_isDragging)
                    DragCompleted?.Invoke(this, EventArgs.Empty);

                _isDragging = false;
                UpdateHandleVisibility();
            }
        }

        private (int row, int col) PixelToClampedCell(double left, double top)
        {
            if (DropGrid == null) return (0, 0);
            int col = (int)(left / DropGrid.CellSize);
            int row = (int)(top / DropGrid.CellSize);
            col = Math.Max(0, Math.Min(col, DropGrid.ColumnCount - (int)CellSpan.Width));
            row = Math.Max(0, Math.Min(row, DropGrid.RowCount - (int)CellSpan.Height));
            return (row, col);
        }

        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ParentCanvas == null || DropGrid == null) return;

            e.Handled = true;
            _isResizing = true;
            _startMousePoint = e.GetPosition(ParentCanvas);
            _startElementSize = CellSpan;

            Panel.SetZIndex(this, 999);
            CaptureMouse();
            ResizeStarted?.Invoke(this, new ResizeEventArgs(_startElementSize, _startElementSize));
        }

        private void HandleResizeMove(MouseEventArgs e)
        {
            if (!_isResizing || e.LeftButton != MouseButtonState.Pressed || ParentCanvas == null || DropGrid == null)
                return;

            Vector delta = e.GetPosition(ParentCanvas) - _startMousePoint;

            int newW = (int)Math.Max(MinCellSpan.Width, _startElementSize.Width + (int)Math.Round(delta.X / DropGrid.CellSize));
            int newH = (int)Math.Max(MinCellSpan.Height, _startElementSize.Height + (int)Math.Round(delta.Y / DropGrid.CellSize));

            var currentPos = DropGrid.GetCellPosition(new Point(Canvas.GetLeft(this), Canvas.GetTop(this)));
            if (!currentPos.HasValue) return;

            newW = Math.Min(newW, DropGrid.ColumnCount - currentPos.Value.col);
            newH = Math.Min(newH, DropGrid.RowCount - currentPos.Value.row);

            var newSize = new Size(newW, newH);
            if (!WouldOverlapSiblings(currentPos.Value, newSize) && newSize != CellSpan)
            {
                CellSpan = newSize;
                ResizeDelta?.Invoke(this, new ResizeEventArgs(_startElementSize, newSize));
            }

            e.Handled = true;
        }

        private void HandleResizeUp(MouseButtonEventArgs e)
        {
            if (!_isResizing) return;
            _isResizing = false;
            ReleaseMouseCapture();
            Panel.SetZIndex(this, 0);
            ResizeCompleted?.Invoke(this, new ResizeEventArgs(_startElementSize, CellSpan));
            UpdateHandleVisibility();
            e.Handled = true;
        }

        private bool WouldOverlapSiblings((int row, int col) position, Size newSize)
        {
            if (ParentCanvas == null || DropGrid == null) return false;

            int endRow = position.row + (int)newSize.Height - 1;
            int endCol = position.col + (int)newSize.Width - 1;

            foreach (var child in ParentCanvas.Children.OfType<DraggableItemControl>())
            {
                if (child == this) continue;

                var otherPos = DropGrid.GetCellPosition(new Point(
                    double.IsNaN(Canvas.GetLeft(child)) ? 0 : Canvas.GetLeft(child),
                    double.IsNaN(Canvas.GetTop(child)) ? 0 : Canvas.GetTop(child)));

                if (!otherPos.HasValue) continue;

                bool rowOverlap = position.row <= otherPos.Value.row + (int)child.CellSpan.Height - 1
                               && endRow >= otherPos.Value.row;
                bool colOverlap = position.col <= otherPos.Value.col + (int)child.CellSpan.Width - 1
                               && endCol >= otherPos.Value.col;

                if (rowOverlap && colOverlap) return true;
            }
            return false;
        }

        private static bool IsAncestorOf(DependencyObject ancestor, DependencyObject target)
        {
            DependencyObject? current = target;
            while (current != null)
            {
                if (current == ancestor) return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            bool ctrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (ctrlPressed)
            {
                e.Handled = true;
                ShowItemContextMenu(e);
            }
            else
            {
                base.OnPreviewMouseRightButtonDown(e);
            }
        }

        private void ShowItemContextMenu(MouseButtonEventArgs e)
        {
            var settings = DataContext as ToolItemSettings;
            if (settings == null) return;

            var contextMenu = new ContextMenu();

            var settingsItem = new MenuItem { Header = "設定" };
            settingsItem.Click += (s, args) => OpenItemSettings(settings);
            contextMenu.Items.Add(settingsItem);

            var hideItem = new MenuItem { Header = "非表示" };
            hideItem.Click += (s, args) => HideItem(settings);
            contextMenu.Items.Add(hideItem);

            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }

        private void HideItem(ToolItemSettings settings)
        {
            ToolBoxSettings.Default.Items.Remove(settings);
        }

        private void OpenItemSettings(ToolItemSettings settings)
        {
            var draggableItem = DiscoverDraggableItems()
                .FirstOrDefault(i => i.SettingsType == settings.GetType());

            if (draggableItem?.SettingsViewType != null)
            {
                var panel = (FrameworkElement)Activator.CreateInstance(draggableItem.SettingsViewType)!;
                panel.DataContext = settings;

                var adornerUtil = new AdornerUtil();
                adornerUtil.Toggle(this, () => panel);
            }
        }

        private static List<DraggableItem> DiscoverDraggableItems()
        {
            return [.. typeof(DraggableItem).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(DraggableItem).IsAssignableFrom(t))
                .Select(t => (DraggableItem)Activator.CreateInstance(t)!)];
        }

        public class ResizeEventArgs(Size startSize, Size currentSize) : EventArgs
        {
            public Size StartSize { get; } = startSize;
            public Size CurrentSize { get; } = currentSize;
        }
    }
}
