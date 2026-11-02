using System.Numerics;
using System.Windows;
using ToolBox.Commons;
using ToolBox.Items.ScreenAlign.Settings;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.ScreenAlign
{
    internal static class ScreenAlignCommand
    {
        public const string DistributeHorizontalCommandName = "ScreenDistributeHorizontalCommand";
        public const string DistributeVerticalCommandName = "ScreenDistributeVerticalCommand";

        private const double Tolerance = 1e-6;

        public static string CommandName(PlacementPosition position) => $"ScreenAlign{position.Name}Command";

        public static bool CanAlign(IToolBoxContext ctx) =>
            ctx.SelectedItems?.OfType<IVideoItem>().Any() == true
            && ctx.Timeline != null && ctx.Scenes != null && ctx.UndoRedoManager != null;

        public static bool CanDistribute(IToolBoxContext ctx) =>
            ctx.SelectedItems?.OfType<IVideoItem>().Count() >= 3 && ctx.Timeline != null && ctx.UndoRedoManager != null;

        private sealed record Target(IVideoItem Item, int Frame, Vector2 Origin, Vector2 Min, Vector2 Max);

        public static void Align(IToolBoxContext ctx, PlacementPosition position)
        {
            if (ctx.UndoRedoManager is not { } undoRedoManager || ctx.Timeline is not { } timeline || ctx.Scenes is not { } scenes)
                return;

            var items = ctx.SelectedItems?.OfType<IVideoItem>().ToList() ?? [];
            if (items.Count == 0)
                return;

            using var measurer = ItemBoundsMeasurer.Create(timeline, scenes);
            if (measurer == null)
            {
                MessageBox.Show(Texts.ScreenAlignMeasureFailed, Texts.ScreenAlignDisplayName, MessageBoxButton.OK);
                return;
            }

            var fps = timeline.VideoInfo.FPS;
            var targets = new List<Target>();
            foreach (var item in items)
            {
                if (measurer.Measure(item) is not { } bounds)
                    continue;

                var frame = FrameOf(item, timeline.CurrentFrame);
                var origin = new Vector2(
                    (float)item.X.GetValue(frame, item.Length, fps),
                    (float)item.Y.GetValue(frame, item.Length, fps));
                targets.Add(new Target(item, frame, origin, origin + bounds.Min, origin + bounds.Max));
            }

            var settings = ctx.FindItemSettings<ScreenAlignSettings>();
            var reference = settings?.Reference ?? AlignReference.Screen;
            var key = reference == AlignReference.TopItem
                ? targets.OrderBy(target => target.Item.Layer).ThenBy(target => target.Item.Frame).FirstOrDefault()
                : null;

            var halfSize = new Vector2(timeline.VideoInfo.Width / 2f, timeline.VideoInfo.Height / 2f);
            var (min, max) = reference switch
            {
                AlignReference.Selection when targets.Count > 0 =>
                    (targets.Select(t => t.Min).Aggregate(Vector2.Min), targets.Select(t => t.Max).Aggregate(Vector2.Max)),
                AlignReference.TopItem when key != null => (key.Min, key.Max),
                _ => (-halfSize, halfSize),
            };

            var margin = new Vector2((float)(settings?.MarginX ?? 0), (float)(settings?.MarginY ?? 0));
            var alignCenter = reference == AlignReference.Screen || position.IsCenter;

            foreach (var target in targets.Where(target => target != key))
            {
                double? dx = position.Horizontal switch
                {
                    HorizontalAnchor.Left => min.X + margin.X - target.Min.X,
                    HorizontalAnchor.Right => max.X - margin.X - target.Max.X,
                    _ when alignCenter => (min.X + max.X - target.Min.X - target.Max.X) / 2,
                    _ => null,
                };

                double? dy = position.Vertical switch
                {
                    VerticalAnchor.Top => min.Y + margin.Y - target.Min.Y,
                    VerticalAnchor.Bottom => max.Y - margin.Y - target.Max.Y,
                    _ when alignCenter => (min.Y + max.Y - target.Min.Y - target.Max.Y) / 2,
                    _ => null,
                };

                Shift(target.Item.X, target.Item, target.Frame, target.Origin.X, dx);
                Shift(target.Item.Y, target.Item, target.Frame, target.Origin.Y, dy);
            }

            undoRedoManager.Record();

            if (targets.Count < items.Count)
                MessageBox.Show(Texts.ScreenAlignMeasureFailed, Texts.ScreenAlignDisplayName, MessageBoxButton.OK);
        }

        public static void Distribute(IToolBoxContext ctx, bool horizontal)
        {
            if (ctx.UndoRedoManager is not { } undoRedoManager || ctx.Timeline is not { } timeline)
                return;

            var fps = timeline.VideoInfo.FPS;
            var targets = (ctx.SelectedItems?.OfType<IVideoItem>() ?? [])
                .Select(item =>
                {
                    var animation = horizontal ? item.X : item.Y;
                    var frame = FrameOf(item, timeline.CurrentFrame);
                    return (Item: item, Animation: animation, Frame: frame, Value: animation.GetValue(frame, item.Length, fps));
                })
                .OrderBy(target => target.Value)
                .ToList();

            if (targets.Count < 3)
                return;

            var first = targets[0].Value;
            var step = (targets[^1].Value - first) / (targets.Count - 1);

            for (var i = 1; i < targets.Count - 1; i++)
            {
                var target = targets[i];
                Shift(target.Animation, target.Item, target.Frame, target.Value, first + step * i - target.Value);
            }

            undoRedoManager.Record();
        }

        private static int FrameOf(IItem item, int currentFrame) =>
            Math.Clamp(currentFrame - item.Frame, 0, Math.Max(0, item.Length - 1));

        private static void Shift(Animation animation, IItem item, int frame, double current, double? delta)
        {
            if (delta is { } d && Math.Abs(d) > Tolerance)
                AnimationHelper.ShiftAt(animation, item, frame, current, d);
        }
    }
}
