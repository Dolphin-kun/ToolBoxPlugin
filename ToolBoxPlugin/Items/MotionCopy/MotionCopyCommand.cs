using ToolBox.Commons;
using ToolBox.Items.MotionCopy.Settings;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.MotionCopy
{
    internal static class MotionCopyCommand
    {
        public const string CopyCommandName = "MotionCopyCommand";
        public const string PasteCommandName = "MotionPasteCommand";

        private sealed record Track(Animation Animation, double[] Values);

        private sealed record Snapshot(int Length, IReadOnlyList<int> Frames, IReadOnlyDictionary<string, Track> Tracks);

        private static Snapshot? clipboard;

        private static readonly string[] positionNames = [nameof(IVideoItem.X), nameof(IVideoItem.Y), nameof(IVideoItem.Z)];

        private static IEnumerable<(string Name, Animation Animation)> Animations(IVideoItem item) =>
        [
            (nameof(IVideoItem.X), item.X),
            (nameof(IVideoItem.Y), item.Y),
            (nameof(IVideoItem.Z), item.Z),
            (nameof(IVideoItem.Opacity), item.Opacity),
            (nameof(IVideoItem.Zoom), item.Zoom),
            (nameof(IVideoItem.Rotation), item.Rotation),
        ];

        public static bool CanCopy(IToolBoxContext ctx) => ctx.SelectedItems?.OfType<IVideoItem>().Any() == true;

        public static bool CanPaste(IToolBoxContext ctx) =>
            clipboard != null && ctx.UndoRedoManager != null && ctx.SelectedItems?.OfType<IVideoItem>().Any() == true;

        public static void Copy(IToolBoxContext ctx)
        {
            if (ctx.SelectedItems?.OfType<IVideoItem>().OrderBy(item => item.Layer).ThenBy(item => item.Frame).FirstOrDefault() is not { } source)
                return;

            clipboard = new Snapshot(
                source.Length,
                [.. source.KeyFrames.Frames],
                Animations(source).ToDictionary(
                    entry => entry.Name,
                    entry =>
                    {
                        var clone = new Animation();
                        clone.CopyFrom(entry.Animation);
                        return new Track(clone, [.. entry.Animation.ActiveValues.Select(value => value.Value)]);
                    }));
        }

        public static void Paste(IToolBoxContext ctx)
        {
            if (clipboard is not { } snapshot || ctx.UndoRedoManager is not { } undoRedoManager)
                return;

            var settings = ctx.FindItemSettings<MotionCopySettings>();
            var names = SelectedNames(settings).ToHashSet();
            if (names.Count == 0)
                return;

            foreach (var item in ctx.SelectedItems?.OfType<IVideoItem>().ToList() ?? [])
                Paste(item, snapshot, names, settings?.FitLength ?? true, settings?.KeepPosition ?? true);

            undoRedoManager.Record();
        }

        private static IEnumerable<string> SelectedNames(MotionCopySettings? settings)
        {
            if (settings?.CopyPosition ?? true)
            {
                foreach (var name in positionNames)
                    yield return name;
            }

            if (settings?.CopyOpacity ?? true)
                yield return nameof(IVideoItem.Opacity);
            if (settings?.CopyZoom ?? true)
                yield return nameof(IVideoItem.Zoom);
            if (settings?.CopyRotation ?? true)
                yield return nameof(IVideoItem.Rotation);
        }

        private static void Paste(IVideoItem item, Snapshot snapshot, HashSet<string> names, bool fitLength, bool keepPosition)
        {
            var targets = Animations(item).Where(entry => names.Contains(entry.Name)).ToList();
            var usesKeyFrames = targets.Any(entry => snapshot.Tracks[entry.Name].Animation.AnimationType.IsKeyFrameSupported());

            var kept = MapFrames(snapshot, item.Length, fitLength);
            if (usesKeyFrames)
                SyncKeyFrames(item.KeyFrames, [.. kept.Select(entry => entry.Frame)]);

            foreach (var (name, animation) in targets)
            {
                var track = snapshot.Tracks[name];
                var origin = animation.GetFirstValue();

                animation.CopyFrom(track.Animation);

                if (track.Animation.AnimationType.IsKeyFrameSupported() && track.Values.Length >= 2)
                {
                    double[] values = [track.Values[0], .. kept.Select(entry => track.Values[entry.Index + 1]), track.Values[^1]];
                    for (var i = 0; i < values.Length && i < animation.Values.Count; i++)
                        animation.Values[i].Value = values[i];
                }

                if (keepPosition && positionNames.Contains(name) && track.Values.Length > 0)
                    animation.AddToEachValues(origin - track.Values[0]);
            }
        }

        private static List<(int Index, int Frame)> MapFrames(Snapshot snapshot, int length, bool fitLength)
        {
            var scale = fitLength && snapshot.Length > 1 ? (length - 1.0) / (snapshot.Length - 1) : 1.0;
            var result = new List<(int Index, int Frame)>();

            for (var i = 0; i < snapshot.Frames.Count; i++)
            {
                var frame = (int)Math.Round(snapshot.Frames[i] * scale);
                if (frame <= 0 || frame >= length - 1 || result.Any(entry => entry.Frame == frame))
                    continue;

                result.Add((i, frame));
            }

            return result;
        }

        private static void SyncKeyFrames(KeyFrames keyFrames, IReadOnlyCollection<int> frames)
        {
            foreach (var frame in frames.Where(frame => !keyFrames.Frames.Contains(frame)))
                keyFrames.Insert(frame);

            for (var i = keyFrames.Frames.Count - 1; i >= 0; i--)
            {
                if (!frames.Contains(keyFrames.Frames[i]))
                    keyFrames.RemoveAt(i);
            }
        }
    }
}
