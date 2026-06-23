using System.Linq;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items.Group
{
    internal static class GroupCommand
    {
        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any();
        }

        public static void InsertIntoGroup(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null) return;
            if (undoRedoManager is null) return;

            var items = selectedItems.ToList();
            if (items.Count == 0) return;

            int minFrame = items.Min(item => item.Frame);
            int maxFrame = items.Max(item => item.Frame + item.Length);

            int minLayer = items.Min(item => item.Layer);
            int maxLayer = items.Max(item => item.Layer);
            int layerRange = maxLayer - minLayer + 1;

            try
            {
                foreach (var item in items)
                {
                    item.Layer += 1;
                }

                var groupItem = new GroupItem
                {
                    Frame = minFrame,
                    Length = maxFrame - minFrame,
                    Layer = minLayer,
                    GroupRange = layerRange
                };

                timeline.TryAddItems([groupItem], groupItem.Frame, groupItem.Layer);
            }
            finally
            {
                timeline.ResolveAllItemsCollision();
                timeline.RefreshTimelineLengthAndMaxLayer();
                undoRedoManager.Record();
            }
        }
    }
}
