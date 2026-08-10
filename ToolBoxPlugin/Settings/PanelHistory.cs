using ToolBox.Commons;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Settings
{
    public sealed class PanelHistory : Bindable, IDisposable
    {
        private const int MaxDepth = 50;

        private readonly ToolBoxPanel panel;
        private readonly PanelItemWatcher watcher;
        private readonly List<string> undoStack = [];
        private readonly List<string> redoStack = [];

        private string currentSnapshot;
        private bool isRestoring;

        public PanelHistory(ToolBoxPanel panel)
        {
            this.panel = panel;
            currentSnapshot = panel.ToJson();

            watcher = new PanelItemWatcher(
                panel,
                [nameof(ToolItemSettings.Position), nameof(ToolItemSettings.Size), nameof(ToolItemSettings.IsLocked)],
                Commit);
        }

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public void Reset()
        {
            undoStack.Clear();
            redoStack.Clear();
            currentSnapshot = panel.ToJson();
            watcher.Rewatch();
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
                watcher.Rewatch();
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

        public void Dispose() => watcher.Dispose();
    }
}
