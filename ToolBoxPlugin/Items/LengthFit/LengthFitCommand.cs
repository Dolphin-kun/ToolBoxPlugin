using ToolBox.Commons;
using ToolBox.Items.LengthFit.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.LengthFit
{
    internal static class LengthFitCommand
    {
        public const string ToNextCommandName = "LengthFitToNextCommand";
        public const string LongestCommandName = "LengthFitLongestCommand";
        public const string ShortestCommandName = "LengthFitShortestCommand";
        public const string ToSeekCommandName = "LengthFitToSeekCommand";

        public static bool CanExecute(IToolBoxContext ctx) =>
            ctx.SelectedItems?.Count > 0 && ctx.Timeline != null && ctx.UndoRedoManager != null;

        public static bool CanMatch(IToolBoxContext ctx) =>
            ctx.SelectedItems?.Count > 1 && ctx.Timeline != null && ctx.UndoRedoManager != null;

        public static void ToNext(IToolBoxContext ctx)
        {
            var gap = ctx.FindItemSettings<LengthFitSettings>()?.Gap ?? 0;
            Apply(ctx, (timeline, item) => timeline.Items
                .Where(other => other.Layer == item.Layer && other.Frame > item.Frame)
                .Select(other => (int?)(other.Frame - gap - item.Frame))
                .Min());
        }

        public static void Longest(IToolBoxContext ctx)
        {
            var length = ctx.SelectedItems?.Max(item => item.Length);
            Apply(ctx, (_, _) => length);
        }

        public static void Shortest(IToolBoxContext ctx)
        {
            var length = ctx.SelectedItems?.Min(item => item.Length);
            Apply(ctx, (_, _) => length);
        }

        public static void ToSeek(IToolBoxContext ctx) =>
            Apply(ctx, (timeline, item) => timeline.CurrentFrame - item.Frame);

        private static void Apply(IToolBoxContext ctx, Func<Timeline, IItem, int?> lengthOf)
        {
            if (ctx.Timeline is not { } timeline || ctx.UndoRedoManager is not { } undoRedoManager || ctx.SelectedItems is not { Count: > 0 } items)
                return;

            var changed = false;
            foreach (var item in items.ToList())
            {
                if (lengthOf(timeline, item) is not { } length || length < 1 || length == item.Length)
                    continue;

                if (item.KeyFrames.Frames.Any(frame => frame >= length - 1))
                    item.KeyFrames.RemovesRight(length - 1);

                item.Length = length;
                changed = true;
            }

            if (!changed)
                return;

            timeline.ResolveAllItemsCollision();
            timeline.RefreshTimelineLengthAndMaxLayer();
            undoRedoManager.Record();
        }
    }
}
