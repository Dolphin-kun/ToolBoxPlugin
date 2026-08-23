using System.Linq;
using ToolBox.Items.Staircase.Settings;
using ToolBox.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;
using ToolBox.Commons;

namespace ToolBox.Items.Staircase
{
    internal static class StaircaseItemCommand
    {
        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any();
        }

        public static void Staircase(IToolBoxContext ctx, IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null) return;
            if (undoRedoManager is null) return;

            var items = selectedItems.ToList();
            if (items.Count == 0) return;

            var maxLayer = items.Max(item => item.Layer);
            var settings = GetSettings(ctx);

            foreach (var item in items)
            {
                var relativeLayer = maxLayer - item.Layer;
                item.Frame += GetStep(relativeLayer, settings);
            }

            timeline.ResolveAllItemsCollision();
            timeline.RefreshTimelineLengthAndMaxLayer();
            undoRedoManager.Record();
        }

        public static void ReverseStaircase(IToolBoxContext ctx, IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null) return;
            if (undoRedoManager is null) return;

            var items = selectedItems.ToList();
            if (items.Count == 0) return;

            var minLayer = items.Min(item => item.Layer);
            var settings = GetSettings(ctx);

            foreach (var item in items)
            {
                var relativeLayer = item.Layer - minLayer;
                item.Frame += GetStep(relativeLayer, settings);
            }

            timeline.ResolveAllItemsCollision();
            timeline.RefreshTimelineLengthAndMaxLayer();
            undoRedoManager.Record();
        }

        private static StaircaseSettings? GetSettings(IToolBoxContext ctx)
            => ctx.FindItemSettings<StaircaseSettings>();

        private static int GetStep(int relativeLayer, StaircaseSettings? settings)
        {
            var offset = settings?.Offset ?? 20;
            var amount = Math.Max(1, settings?.Amount ?? 1);

            return relativeLayer / amount * offset;
        }
    }
}
