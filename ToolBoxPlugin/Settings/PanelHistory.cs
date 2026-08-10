using System.Collections.Specialized;
using System.ComponentModel;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Settings
{
    public sealed class PanelHistory : Bindable, IDisposable
    {
        private const int MaxDepth = 50;

        private readonly ToolBoxPanel panel;
        private readonly List<string> undoStack = [];
        private readonly List<string> redoStack = [];
        private readonly HashSet<ToolItemSettings> watchedItems = [];

        private string currentSnapshot;

        private bool isRestoring;

        public PanelHistory(ToolBoxPanel panel)
        {
            this.panel = panel;
            currentSnapshot = panel.ToJson();

            panel.Items.CollectionChanged += OnItemsChanged;
            WatchAllItems();
        }

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public void Reset()
        {
            undoStack.Clear();
            redoStack.Clear();
            currentSnapshot = panel.ToJson();
            WatchAllItems();
            NotifyAvailability();
        }

        public void Undo() => Step(undoStack, redoStack);

        public void Redo() => Step(redoStack, undoStack);

        private void Step(List<string> from, List<string> to)
        {
            if (from.Count == 0)
                return;

            var target = from[^1];
            from.RemoveAt(from.Count - 1);
            to.Add(currentSnapshot);

            isRestoring = true;
            try
            {
                panel.LoadJson(target);
                currentSnapshot = target;
                WatchAllItems();
            }
            finally
            {
                isRestoring = false;
            }

            NotifyAvailability();
        }

        private void Commit()
        {
            if (isRestoring)
                return;

            var snapshot = panel.ToJson();
            if (snapshot == currentSnapshot)
                return;

            undoStack.Add(currentSnapshot);
            if (undoStack.Count > MaxDepth)
                undoStack.RemoveAt(0);

            redoStack.Clear();
            currentSnapshot = snapshot;
            NotifyAvailability();
        }

        private void NotifyAvailability()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            WatchAllItems();
            Commit();
        }

        private void WatchAllItems()
        {
            foreach (var item in watchedItems)
                item.PropertyChanged -= OnItemPropertyChanged;

            watchedItems.Clear();

            foreach (var item in panel.Items)
            {
                item.PropertyChanged += OnItemPropertyChanged;
                watchedItems.Add(item);
            }
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ToolItemSettings.Position)
                or nameof(ToolItemSettings.Size)
                or nameof(ToolItemSettings.IsLocked))
            {
                Commit();
            }
        }

        public void Dispose()
        {
            panel.Items.CollectionChanged -= OnItemsChanged;
            foreach (var item in watchedItems)
                item.PropertyChanged -= OnItemPropertyChanged;
            watchedItems.Clear();
        }
    }
}
