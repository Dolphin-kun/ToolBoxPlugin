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
            }

            di.DropGrid = DropGrid;
            di.DragDelta += OnItemDragDelta;
            di.DragCompleted += OnItemDragCompleted;
            di.ResizeCompleted += OnItemResizeCompleted;
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is DraggableItemControl di)
            {
                di.DragDelta -= OnItemDragDelta;
                di.DragCompleted -= OnItemDragCompleted;
                di.ResizeCompleted -= OnItemResizeCompleted;
            }
        }

        private void OnGeneratorStatusChanged(object? sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                ApplyAllItemPositions();
        }

        private void ApplyAllItemPositions()
        {
            if (DropGrid == null) return;

            var items = GetAllDraggableItems().ToList();
            var occupied = new HashSet<(int r, int c)>();

            foreach (var di in items)
            {
                if (di.DataContext is not ToolItemSettings setting) continue;

                int prefRow = (int)setting.Position.Y;
                int prefCol = (int)setting.Position.X;

                var (row, col) = FindFreePosition(di.CellSpan, prefRow, prefCol, occupied);

                MarkOccupied(occupied, row, col, di.CellSpan);

                var pos = DropGrid.GetCellTopLeft(row, col);
                Canvas.SetLeft(di, pos.X);
                Canvas.SetTop(di, pos.Y);
            }
        }

        private (int row, int col) FindFreePosition(Size span, int prefRow, int prefCol, HashSet<(int, int)> occupied)
        {
            if (DropGrid == null) return (prefRow, prefCol);

            int spanW = (int)span.Width;
            int spanH = (int)span.Height;
            int maxDist = Math.Max(DropGrid.RowCount, DropGrid.ColumnCount);

            for (int dist = 0; dist <= maxDist; dist++)
            {
                for (int dy = -dist; dy <= dist; dy++)
                {
                    for (int dx = -dist; dx <= dist; dx++)
                    {
                        if (Math.Abs(dx) != dist && Math.Abs(dy) != dist) continue;

                        int row = prefRow + dy;
                        int col = prefCol + dx;

                        if (row < 0 || row > DropGrid.RowCount - spanH) continue;
                        if (col < 0 || col > DropGrid.ColumnCount - spanW) continue;

                        if (!IsOccupied(occupied, row, col, span))
                            return (row, col);
                    }
                }
            }

            return (prefRow, prefCol);
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

        private void OnItemDragDelta(object? sender, EventArgs e)
        {
            if (sender is not DraggableItemControl di || DropGrid == null) return;

            var pos = DropGrid.GetCellPosition(new Point(Canvas.GetLeft(di), Canvas.GetTop(di)));
            if (pos.HasValue)
                DropGrid.HighlightCell(pos.Value.row, pos.Value.col, di.CellSpan);
        }

        private void OnItemDragCompleted(object? sender, EventArgs e)
        {
            if (sender is not DraggableItemControl di || DropGrid == null) return;

            DropGrid.ClearHighlight();

            var current = DropGrid.GetCellPosition(new Point(Canvas.GetLeft(di), Canvas.GetTop(di)));
            if (!current.HasValue) return;

            var (prefRow, prefCol) = current.Value;
            var occupied = BuildOccupiedSet(exclude: di);
            var (finalRow, finalCol) = FindFreePosition(di.CellSpan, prefRow, prefCol, occupied);

            var finalPos = DropGrid.GetCellTopLeft(finalRow, finalCol);
            Canvas.SetLeft(di, finalPos.X);
            Canvas.SetTop(di, finalPos.Y);

            if (di.DataContext is ToolItemSettings setting)
                setting.Position = new Point(finalCol, finalRow);
        }

        private void OnItemResizeCompleted(object? sender, DraggableItemControl.ResizeEventArgs e)
        {
            if (sender is not DraggableItemControl di) return;
            if (di.DataContext is ToolItemSettings setting)
            {
                setting.Size = e.CurrentSize;
            }
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
                var pos = DropGrid.GetCellPosition(new Point(Canvas.GetLeft(di), Canvas.GetTop(di)));
                if (pos.HasValue)
                    MarkOccupied(occupied, pos.Value.row, pos.Value.col, di.CellSpan);
            }

            return occupied;
        }
    }
}
