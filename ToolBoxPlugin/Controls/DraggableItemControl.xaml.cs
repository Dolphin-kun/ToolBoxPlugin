using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ToolBox.Commons;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Controls
{
    public partial class DraggableItemControl : UserControl
    {
        public event EventHandler? DragDelta;
        public event EventHandler? DragCompleted;
        public event EventHandler<ResizeEventArgs>? ResizeCompleted;

        private Point startMousePoint;
        private Point startElementPosition;
        private Size startElementSize;
        private bool isDragging;
        private bool isResizing;
        private Canvas? parentCanvas;
        private Window? parentWindow;

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
            DataContextChanged += OnDataContextChanged;
        }

        private ToolItemSettings? appearanceSource;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            appearanceSource?.PropertyChanged -= OnSettingsPropertyChanged;

            appearanceSource = e.NewValue as ToolItemSettings;

            appearanceSource?.PropertyChanged += OnSettingsPropertyChanged;

            ApplyIconBrushOverride();
        }

        private void OnSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ToolItemSettings.AppearanceForeground)
                or nameof(ToolItemSettings.UseCustomAppearance))
            {
                ApplyIconBrushOverride();
            }
        }

        private void ApplyIconBrushOverride()
        {
            var key = YukkuriMovieMaker.Theme.YMM4Colors.IconBrushKey;
            var brush = appearanceSource?.AppearanceForeground;

            if (brush == null)
                Resources.Remove(key);
            else
                Resources[key] = brush;
        }

        private static void OnCellSpanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DraggableItemControl item) item.UpdateSize();
        }

        private static void OnDropGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DraggableItemControl item) return;

            if (e.OldValue is DropGrid oldGrid) oldGrid.GridChanged -= item.OnDropGridSizeChanged;
            if (e.NewValue is DropGrid newGrid) newGrid.GridChanged += item.OnDropGridSizeChanged;

            item.UpdateSize();
        }

        private void OnDropGridSizeChanged(object? sender, EventArgs e) => UpdateSize();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            parentCanvas = VisualTreeHelper.GetParent(this) as Canvas;
            UpdateSize();
            parentWindow = Window.GetWindow(this);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachWindowKeyHandlers();
            DropGrid?.GridChanged -= OnDropGridSizeChanged;
        }

        private void UpdateSize()
        {
            if (DropGrid == null) return;
            Width = CellSpan.Width * DropGrid.CellSize;
            Height = CellSpan.Height * DropGrid.CellSize;
        }

        private Canvas? ParentCanvas => parentCanvas;

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            AttachWindowKeyHandlers();
            UpdateHandleVisibility();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!isDragging && !isResizing)
            {
                DetachWindowKeyHandlers();
                ResizeHandle.Visibility = Visibility.Collapsed;
            }
        }

        private void AttachWindowKeyHandlers()
        {
            if (parentWindow == null) return;
            parentWindow.KeyDown -= OnParentWindowKeyChanged;
            parentWindow.KeyUp -= OnParentWindowKeyChanged;
            parentWindow.KeyDown += OnParentWindowKeyChanged;
            parentWindow.KeyUp += OnParentWindowKeyChanged;
        }

        private void DetachWindowKeyHandlers()
        {
            if (parentWindow == null) return;
            parentWindow.KeyDown -= OnParentWindowKeyChanged;
            parentWindow.KeyUp -= OnParentWindowKeyChanged;
        }

        private void OnParentWindowKeyChanged(object sender, KeyEventArgs e) => UpdateHandleVisibility();

        private bool IsLocked => DataContext is ToolItemSettings settings && settings.IsLocked;

        private void UpdateHandleVisibility()
        {
            if (isResizing) return;
            bool ctrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (IsMouseOver && ctrlPressed && IsLocked)
            {
                ResizeHandle.Visibility = Visibility.Collapsed;
                Cursor = Cursors.No;
            }
            else if (IsMouseOver && ctrlPressed)
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
            if (!ctrlPressed || IsLocked) return;

            if (e.OriginalSource is DependencyObject src && IsAncestorOf(ResizeHandle, src)) return;

            var canvas = ParentCanvas;
            if (canvas == null) return;

            startMousePoint = e.GetPosition(canvas);
            startElementPosition = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            if (double.IsNaN(startElementPosition.X)) startElementPosition.X = 0;
            if (double.IsNaN(startElementPosition.Y)) startElementPosition.Y = 0;

            isDragging = false;
            Panel.SetZIndex(this, 999);
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (isResizing) { HandleResizeMove(e); return; }

            var canvas = ParentCanvas;
            if (!IsMouseCaptured || canvas == null || e.LeftButton != MouseButtonState.Pressed) return;

            Point current = e.GetPosition(canvas);

            if (!isDragging)
            {
                Vector diff = startMousePoint - current;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                    isDragging = true;
            }

            if (isDragging && DropGrid != null)
            {
                double newLeft = startElementPosition.X + (current.X - startMousePoint.X);
                double newTop = startElementPosition.Y + (current.Y - startMousePoint.Y);

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

            if (isResizing) { HandleResizeUp(e); return; }

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                Panel.SetZIndex(this, 0);

                if (isDragging)
                    DragCompleted?.Invoke(this, EventArgs.Empty);

                isDragging = false;
                UpdateHandleVisibility();
            }
        }

        private (int row, int col) PixelToClampedCell(double left, double top)
        {
            if (DropGrid == null) return (0, 0);
            int col = Controls.DropGrid.ToCellIndex(left, DropGrid.CellSize);
            int row = Controls.DropGrid.ToCellIndex(top, DropGrid.CellSize);
            col = Math.Max(0, Math.Min(col, DropGrid.ColumnCount - (int)CellSpan.Width));
            row = Math.Max(0, Math.Min(row, DropGrid.RowCount - (int)CellSpan.Height));
            return (row, col);
        }

        private void ResizeHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ParentCanvas == null || DropGrid == null || IsLocked) return;

            e.Handled = true;
            isResizing = true;
            startMousePoint = e.GetPosition(ParentCanvas);
            startElementSize = CellSpan;

            Panel.SetZIndex(this, 999);
            CaptureMouse();
        }

        private void HandleResizeMove(MouseEventArgs e)
        {
            if (!isResizing || e.LeftButton != MouseButtonState.Pressed || ParentCanvas == null || DropGrid == null)
                return;

            Vector delta = e.GetPosition(ParentCanvas) - startMousePoint;

            int newW = (int)Math.Max(MinCellSpan.Width, startElementSize.Width + (int)Math.Round(delta.X / DropGrid.CellSize));
            int newH = (int)Math.Max(MinCellSpan.Height, startElementSize.Height + (int)Math.Round(delta.Y / DropGrid.CellSize));

            var currentPos = DropGrid.GetCellPosition(new Point(Canvas.GetLeft(this), Canvas.GetTop(this)));
            if (!currentPos.HasValue) return;

            newW = Math.Min(newW, DropGrid.ColumnCount - currentPos.Value.col);
            newH = Math.Min(newH, DropGrid.RowCount - currentPos.Value.row);

            var newSize = new Size(newW, newH);
            if (!WouldOverlapSiblings(currentPos.Value, newSize) && newSize != CellSpan)
            {
                CellSpan = newSize;
            }

            e.Handled = true;
        }

        private void HandleResizeUp(MouseButtonEventArgs e)
        {
            if (!isResizing) return;
            isResizing = false;
            ReleaseMouseCapture();
            Panel.SetZIndex(this, 0);
            ResizeCompleted?.Invoke(this, new ResizeEventArgs(startElementSize, CellSpan));
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
                ShowItemContextMenu();
            }
            else
            {
                base.OnPreviewMouseRightButtonDown(e);
            }
        }

        private void ShowItemContextMenu()
        {
            if (DataContext is not ToolItemSettings settings) return;

            var contextMenu = new ContextMenu();

            var settingsItem = new MenuItem { Header = Texts.CommonSettings };
            settingsItem.Click += (s, args) => OpenItemSettings(settings);
            contextMenu.Items.Add(settingsItem);

            var copyItem = new MenuItem { Header = Texts.CommonCopy };
            copyItem.Click += (s, args) => ItemClipboard.Copy(settings);
            contextMenu.Items.Add(copyItem);

            contextMenu.Items.Add(new Separator());

            var lockItem = new MenuItem { Header = settings.IsLocked ? Texts.ItemContextUnlock : Texts.ItemContextLock };
            lockItem.Click += (s, args) => settings.IsLocked = !settings.IsLocked;
            contextMenu.Items.Add(lockItem);

            var hideItem = new MenuItem { Header = Texts.ItemContextHide };
            hideItem.Click += (s, args) => HideItem(settings);
            contextMenu.Items.Add(hideItem);

            contextMenu.Closed += (s, args) =>
                Dispatcher.BeginInvoke(() => DropGrid?.RefreshCtrlState());

            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }

        private void HideItem(ToolItemSettings settings)
        {
            FindOwnerPanel()?.HideItem(settings);
        }

        private ToolBoxPanel? FindOwnerPanel()
        {
            DependencyObject? current = this;
            while (current != null)
            {
                if (current is FrameworkElement { DataContext: ToolBoxViewModel vm })
                    return vm.Panel;
                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private void OpenItemSettings(ToolItemSettings settings)
        {
            if (ItemSettingsPanel.Create(settings) is not { } panel) return;

            new AdornerUtil().Show(this, () => panel);
        }

        public class ResizeEventArgs(Size startSize, Size currentSize) : EventArgs
        {
            public Size StartSize { get; } = startSize;
            public Size CurrentSize { get; } = currentSize;
        }
    }
}
