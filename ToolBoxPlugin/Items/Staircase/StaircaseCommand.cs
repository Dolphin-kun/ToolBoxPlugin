using ToolBox.Commons;
using ToolBox.Items.Staircase.Settings;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.Staircase
{
    internal static class StaircaseItemCommand
    {
        public static bool CanExecute(IToolBoxContext ctx) =>
            ctx.SelectedItems?.Any() == true && ctx.Timeline != null && ctx.UndoRedoManager != null;

        public static void Staircase(IToolBoxContext ctx, bool reverse)
        {
            if (ctx.Timeline is not { } timeline || ctx.UndoRedoManager is not { } undoRedoManager)
                return;

            var items = ctx.SelectedItems?.ToList() ?? [];
            if (items.Count == 0)
                return;

            var settings = ctx.FindItemSettings<StaircaseSettings>();
            var offset = settings?.Offset ?? 20;
            var amount = Math.Max(1, settings?.Amount ?? 1);
            var minLayer = items.Min(item => item.Layer);
            var maxLayer = items.Max(item => item.Layer);

            int StepOf(IItem item) => (reverse ? item.Layer - minLayer : maxLayer - item.Layer) / amount;

            var count = items.Max(StepOf) + 1;
            var delays = GetDelays(count, offset, settings);

            foreach (var item in items)
                item.Frame = Math.Max(0, item.Frame + delays[StepOf(item)]);

            timeline.ResolveAllItemsCollision();
            timeline.RefreshTimelineLengthAndMaxLayer();
            undoRedoManager.Record();
        }

        private static int[] GetDelays(int count, int offset, StaircaseSettings? settings)
        {
            var order = settings?.Order ?? StaircaseOrder.Layer;
            var half = (count - 1) / 2.0;
            var shuffled = Enumerable.Range(0, count).OrderBy(_ => Random.Shared.Next()).ToArray();

            double Rank(int step) => order switch
            {
                StaircaseOrder.CenterOut => Math.Abs(step - half),
                StaircaseOrder.EdgesIn => half - Math.Abs(step - half),
                StaircaseOrder.Random => shuffled[step],
                _ => step,
            };

            var max = order is StaircaseOrder.CenterOut or StaircaseOrder.EdgesIn ? half : count - 1;
            var curve = settings?.Curve ?? EasingType.Linear;
            var direction = settings?.Direction ?? EasingMode.In;

            var delays = Enumerable.Range(0, count)
                .Select(step => max <= 0 ? 0 : (int)Math.Round(max * offset * YukkuriMovieMaker.Commons.Easing.GetValue(curve, direction, Rank(step) / max)))
                .ToArray();

            var min = delays.Min();
            return [.. delays.Select(delay => delay - min)];
        }
    }
}
