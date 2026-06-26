using System.Linq;
using ToolBox.Items.Staircase.Settings;
using ToolBox.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items.Staircase
{
    internal static class StaircaseItemCommand
    {
        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any();
        }

        public static void Staircase(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null) return;
            if (undoRedoManager is null) return;

            var items = selectedItems.ToList();
            if (items.Count == 0) return;

            var maxLayer = items.Max(item => item.Layer);

            foreach (var item in items)
            {
                var relativeLayer = maxLayer - item.Layer;
                item.Frame += relativeLayer * GetStaircaseOffset();
            }

            timeline.ResolveAllItemsCollision();
            timeline.RefreshTimelineLengthAndMaxLayer();
            undoRedoManager.Record();
        }

        public static void ReverseStaircase(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null) return;
            if (undoRedoManager is null) return;

            var items = selectedItems.ToList();
            if (items.Count == 0) return;

            var minLayer = items.Min(item => item.Layer);

            foreach (var item in items)
            {
                var relativeLayer = item.Layer - minLayer;
                item.Frame += relativeLayer * GetStaircaseOffset();
            }

            timeline.ResolveAllItemsCollision();
            timeline.RefreshTimelineLengthAndMaxLayer();
            undoRedoManager.Record();
        }

        private static int GetStaircaseOffset()
            => ToolBoxSettings.Default.Items
                .OfType<StaircaseSettings>()
                .FirstOrDefault()?.Offset ?? 20;
    }
}
