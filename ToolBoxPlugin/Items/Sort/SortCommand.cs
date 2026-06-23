using System.Linq;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items.Sort
{
    internal static class SortCommand
    {
        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any();
        }

        public static void SortByFrameAscending(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null || undoRedoManager is null) return;
            var items = selectedItems.ToList();
            if (items.Count == 0) return;

            var minLayer = items.Min(item => item.Layer);
            var sortedItems = items.OrderBy(item => item.Frame).ToList();

            for (int i = 0; i < sortedItems.Count; i++)
            {
                sortedItems[i].Layer = minLayer + i;
            }

            timeline.ResolveAllItemsCollision();
            timeline.RefreshTimelineLengthAndMaxLayer();
            undoRedoManager.Record();
        }

        public static void SortByFrameDescending(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null || undoRedoManager is null) return;
            var items = selectedItems.ToList();
            if (items.Count == 0) return;

            var minLayer = items.Min(item => item.Layer);
            var sortedItems = items.OrderByDescending(item => item.Frame).ToList();

            for (int i = 0; i < sortedItems.Count; i++)
            {
                sortedItems[i].Layer = minLayer + i;
            }

            timeline.ResolveAllItemsCollision();
            timeline.RefreshTimelineLengthAndMaxLayer();
            undoRedoManager.Record();
        }
    }
}
