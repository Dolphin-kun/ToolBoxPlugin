using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ToolBox.Settings;

namespace ToolBox.Controls
{
    public class DraggableCanvas : ItemsControl
    {
        public static readonly DependencyProperty DropGridProperty =
            DependencyProperty.Register(nameof(DropGrid), typeof(DropGrid), typeof(DraggableCanvas),
                new PropertyMetadata(null, OnDropGridChanged));

        public DropGrid? DropGrid
        {
            get => (DropGrid?)GetValue(DropGridProperty);
            set => SetValue(DropGridProperty, value);
        }

        private static void OnDropGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DraggableCanvas canvas) return;
            if (e.OldValue is DropGrid oldGrid)
            {
                oldGrid.GridChanged -= canvas.OnGridChanged;
            }
            if (e.NewValue is DropGrid newGrid)
            {
                newGrid.GridChanged += canvas.OnGridChanged;
                canvas.ApplyAllItemPositions();
            }
        }

        private void OnGridChanged(object? sender, EventArgs e)
        {
            ApplyAllItemPositions();
        }

        private Dictionary<DraggableItemControl, (int row, int col)>? resizeOrigins;
        private bool isCommitting;

        public DraggableCanvas()
        {
            ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        }

        protected override DependencyObject GetContainerForItemOverride()
            => new DraggableItemControl();

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is DraggableItemControl;

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is not DraggableItemControl di) return;

            if (item is ToolItemSettings setting)
            {
                di.CellSpan = setting.Size;
                di.MinCellSpan = setting.MinSize;
                setting.PropertyChanged += OnSettingPropertyChanged;
            }

            di.DropGrid = DropGrid;
            di.DragDelta += OnItemDragDelta;
            di.DragCompleted += OnItemDragCompleted;
            di.ResizeStarted += OnItemResizeStarted;
            di.Resizing += OnItemResizing;
            di.ResizeCompleted += OnItemResizeCompleted;
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (item is ToolItemSettings setting)
                setting.PropertyChanged -= OnSettingPropertyChanged;

            if (element is DraggableItemControl di)
            {
                di.DragDelta -= OnItemDragDelta;
                di.DragCompleted -= OnItemDragCompleted;
                di.ResizeStarted -= OnItemResizeStarted;
                di.Resizing -= OnItemResizing;
                di.ResizeCompleted -= OnItemResizeCompleted;
            }
        }

        private void OnGeneratorStatusChanged(object? sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                ApplyAllItemPositions();
        }

        private void OnSettingPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (isCommitting || sender is not ToolItemSettings setting)
                return;

            if (e.PropertyName is not (nameof(ToolItemSettings.Position) or nameof(ToolItemSettings.Size) or nameof(ToolItemSettings.MinSize)))
                return;

            if (ItemContainerGenerator.ContainerFromItem(setting) is DraggableItemControl di)
            {
                di.CellSpan = setting.Size;
                di.MinCellSpan = setting.MinSize;
            }

            ApplyAllItemPositions();
        }

        private void ApplyAllItemPositions()
        {
            if (DropGrid == null) return;

            var items = GetAllDraggableItems()
                .Where(di => di.DataContext is ToolItemSettings)
                .OrderBy(di => ((ToolItemSettings)di.DataContext).Position.Y)
                .ThenBy(di => ((ToolItemSettings)di.DataContext).Position.X)
                .ToList();

            var occupied = new HashSet<(int r, int c)>();
            var cursor = 0;

            foreach (var di in items)
            {
                var setting = (ToolItemSettings)di.DataContext;
                int row = (int)setting.Position.Y;
                int col = (int)setting.Position.X;

                if (!CanPlace(occupied, row, col, di.CellSpan))
                {
                    var next = FindNextFree(occupied, di.CellSpan, cursor) ?? FindNextFree(occupied, di.CellSpan, 0);
                    (row, col) = next ?? ClampToGrid(row, col, di.CellSpan);
                }

                MarkOccupied(occupied, row, col, di.CellSpan);
                cursor = row * DropGrid.ColumnCount + col;
                SetCell(di, row, col);
            }
        }

        private bool CanPlace(HashSet<(int, int)> occupied, int row, int col, Size span)
        {
            if (DropGrid == null) return false;

            if (row < 0 || row > DropGrid.RowCount - (int)span.Height) return false;
            if (col < 0 || col > DropGrid.ColumnCount - (int)span.Width) return false;

            return !IsOccupied(occupied, row, col, span);
        }

        private (int row, int col)? FindNextFree(HashSet<(int, int)> occupied, Size span, int start)
        {
            if (DropGrid == null || DropGrid.ColumnCount <= 0) return null;

            var count = DropGrid.RowCount * DropGrid.ColumnCount;
            for (var index = Math.Max(0, start); index < count; index++)
            {
                var row = index / DropGrid.ColumnCount;
                var col = index % DropGrid.ColumnCount;
                if (CanPlace(occupied, row, col, span))
                    return (row, col);
            }

            return null;
        }

        private (int row, int col) ClampToGrid(int row, int col, Size span)
        {
            if (DropGrid == null) return (row, col);

            return (Math.Clamp(row, 0, Math.Max(0, DropGrid.RowCount - (int)span.Height)),
                    Math.Clamp(col, 0, Math.Max(0, DropGrid.ColumnCount - (int)span.Width)));
        }

        private bool TryFindFreePosition(Size span, int prefRow, int prefCol, HashSet<(int, int)> occupied, out (int row, int col) found)
        {
            found = (prefRow, prefCol);
            if (DropGrid == null) return false;

            int maxDist = Math.Max(DropGrid.RowCount, DropGrid.ColumnCount) + Math.Max(Math.Abs(prefRow), Math.Abs(prefCol));

            for (int dist = 0; dist <= maxDist; dist++)
            {
                for (int dy = -dist; dy <= dist; dy++)
                {
                    for (int dx = -dist; dx <= dist; dx++)
                    {
                        if (Math.Abs(dx) != dist && Math.Abs(dy) != dist) continue;

                        if (CanPlace(occupied, prefRow + dy, prefCol + dx, span))
                        {
                            found = (prefRow + dy, prefCol + dx);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsOccupied(HashSet<(int, int)> occupied, int row, int col, Size span)
        {
            for (int r = row; r < row + (int)span.Height; r++)
                for (int c = col; c < col + (int)span.Width; c++)
                    if (occupied.Contains((r, c))) return true;
            return false;
        }

        private static void MarkOccupied(HashSet<(int, int)> occupied, int row, int col, Size span)
        {
            for (int r = row; r < row + (int)span.Height; r++)
                for (int c = col; c < col + (int)span.Width; c++)
                    occupied.Add((r, c));
        }

        private static bool Overlaps((int row, int col) a, Size aSpan, (int row, int col) b, Size bSpan) =>
            a.row < b.row + (int)bSpan.Height && b.row < a.row + (int)aSpan.Height
            && a.col < b.col + (int)bSpan.Width && b.col < a.col + (int)aSpan.Width;

        private void SetCell(DraggableItemControl di, int row, int col)
        {
            if (DropGrid == null) return;

            var pos = DropGrid.GetCellTopLeft(row, col);
            Canvas.SetLeft(di, pos.X);
            Canvas.SetTop(di, pos.Y);
        }

        private (int row, int col)? GetCell(DraggableItemControl di) =>
            DropGrid?.GetCellPosition(new Point(
                double.IsNaN(Canvas.GetLeft(di)) ? 0 : Canvas.GetLeft(di),
                double.IsNaN(Canvas.GetTop(di)) ? 0 : Canvas.GetTop(di)));

        private void OnItemDragDelta(object? sender, EventArgs e)
        {
            if (sender is not DraggableItemControl di || DropGrid == null) return;

            var pos = GetCell(di);
            if (pos.HasValue)
                DropGrid.HighlightCell(pos.Value.row, pos.Value.col, di.CellSpan);
        }

        private void OnItemDragCompleted(object? sender, EventArgs e)
        {
            if (sender is not DraggableItemControl di || DropGrid == null) return;

            DropGrid.ClearHighlight();

            var current = GetCell(di);
            if (!current.HasValue) return;

            var (prefRow, prefCol) = current.Value;
            var occupied = BuildOccupiedSet(exclude: di);
            if (!TryFindFreePosition(di.CellSpan, prefRow, prefCol, occupied, out var final))
                final = (prefRow, prefCol);

            SetCell(di, final.row, final.col);

            if (di.DataContext is ToolItemSettings setting)
                setting.Position = new Point(final.col, final.row);
        }

        private void OnItemResizeStarted(object? sender, EventArgs e)
        {
            resizeOrigins = [];

            foreach (var other in GetAllDraggableItems())
            {
                if (other != sender && GetCell(other) is { } cell)
                    resizeOrigins[other] = cell;
            }
        }

        private void OnItemResizing(object? sender, DraggableItemControl.ResizingEventArgs e)
        {
            if (sender is not DraggableItemControl di || resizeOrigins == null || GetCell(di) is not { } at)
            {
                e.Cancel = true;
                return;
            }

            var occupied = new HashSet<(int, int)>();
            MarkOccupied(occupied, at.row, at.col, e.NewSize);

            var placements = new Dictionary<DraggableItemControl, (int row, int col)>();
            var displaced = new List<DraggableItemControl>();

            foreach (var (other, origin) in resizeOrigins.OrderBy(pair => pair.Value.row).ThenBy(pair => pair.Value.col))
            {
                if (!Overlaps(origin, other.CellSpan, at, e.NewSize))
                {
                    MarkOccupied(occupied, origin.row, origin.col, other.CellSpan);
                    placements[other] = origin;
                }
                else if (other.DataContext is ToolItemSettings { IsLocked: true })
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    displaced.Add(other);
                }
            }

            foreach (var other in displaced)
            {
                var origin = resizeOrigins[other];
                var right = at.col + (int)e.NewSize.Width - origin.col;
                var down = at.row + (int)e.NewSize.Height - origin.row;
                var (prefRow, prefCol) = right <= down ? (origin.row, origin.col + right) : (origin.row + down, origin.col);

                if (!TryFindFreePosition(other.CellSpan, prefRow, prefCol, occupied, out var cell))
                {
                    e.Cancel = true;
                    return;
                }

                MarkOccupied(occupied, cell.row, cell.col, other.CellSpan);
                placements[other] = cell;
            }

            foreach (var (other, cell) in placements)
                SetCell(other, cell.row, cell.col);
        }

        private void OnItemResizeCompleted(object? sender, DraggableItemControl.ResizeEventArgs e)
        {
            if (sender is not DraggableItemControl di) return;

            isCommitting = true;
            try
            {
                if (di.DataContext is ToolItemSettings setting)
                    setting.Size = e.CurrentSize;

                foreach (var (other, origin) in resizeOrigins ?? [])
                {
                    if (GetCell(other) is { } cell && cell != origin && other.DataContext is ToolItemSettings moved)
                        moved.Position = new Point(cell.col, cell.row);
                }
            }
            finally
            {
                isCommitting = false;
                resizeOrigins = null;
            }

            ApplyAllItemPositions();
        }

        public IEnumerable<DraggableItemControl> GetAllDraggableItems()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is DraggableItemControl di)
                    yield return di;
            }
        }

        private HashSet<(int, int)> BuildOccupiedSet(DraggableItemControl? exclude = null)
        {
            var occupied = new HashSet<(int, int)>();
            if (DropGrid == null) return occupied;

            foreach (var di in GetAllDraggableItems())
            {
                if (di == exclude) continue;
                if (GetCell(di) is { } pos)
                    MarkOccupied(occupied, pos.row, pos.col, di.CellSpan);
            }

            return occupied;
        }
    }
}
