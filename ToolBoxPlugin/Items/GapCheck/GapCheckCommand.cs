using System.Text;
using System.Windows;
using ToolBox.Commons;
using ToolBox.Items.GapCheck.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.GapCheck
{
    internal static class GapCheckCommand
    {
        private const int MaxListedGaps = 20;

        public static bool CanExecute(Timeline? timeline) => timeline?.Items.Count > 0;

        public static void MoveToNext(IToolBoxContext ctx) => Move(ctx, forward: true);

        public static void MoveToPrevious(IToolBoxContext ctx) => Move(ctx, forward: false);

        private static void Move(IToolBoxContext ctx, bool forward)
        {
            if (ctx.Timeline is not { } timeline)
                return;

            var gaps = FindGaps(ctx, timeline);
            if (gaps.Count == 0)
            {
                Notify(Texts.GapCheckNotFound);
                return;
            }

            var current = timeline.CurrentFrame;

            var target = forward
                ? gaps.FirstOrDefault(gap => gap.Frame > current) ?? gaps[0]
                : gaps.LastOrDefault(gap => gap.Frame < current) ?? gaps[^1];

            timeline.CurrentFrame = target.Frame;
            timeline.SelectItems([target.Before, target.After]);
        }

        public static void ShowSummary(IToolBoxContext ctx)
        {
            if (ctx.Timeline is not { } timeline)
                return;

            var gaps = FindGaps(ctx, timeline);
            if (gaps.Count == 0)
            {
                Notify(Texts.GapCheckNotFound);
                return;
            }

            var text = new StringBuilder();
            text.AppendLine(string.Format(Texts.GapCheckFoundSummary, gaps.Count));
            text.AppendLine();

            foreach (var gap in gaps.Take(MaxListedGaps))
                text.AppendLine(Describe(gap));

            if (gaps.Count > MaxListedGaps)
                text.AppendLine(string.Format(Texts.GapCheckFoundMore, gaps.Count - MaxListedGaps));

            MessageBox.Show(text.ToString(), Texts.GapCheckDisplayName, MessageBoxButton.OK);
        }

        private static string Describe(TimelineGap gap) => gap.Layer is { } layer
            ? string.Format(Texts.GapCheckFoundLine, layer, gap.Frame, gap.EndFrame)
            : string.Format(Texts.GapCheckFoundLineAll, gap.Frame, gap.EndFrame);

        public static IReadOnlyList<TimelineGap> FindGaps(IToolBoxContext ctx, Timeline timeline)
        {
            var settings = ctx.FindItemSettings<GapCheckSettings>();
            var maxGap = Math.Max(1, settings?.MaxGapFrames ?? 2);
            var mode = settings?.Mode ?? GapSearchMode.All;

            var targetLayers = settings?.SelectedLayersOnly == true
                ? TimelineHelper.GetSelectedLayers(timeline).ToHashSet()
                : null;

            var items = timeline.Items
                .Where(item => targetLayers == null || targetLayers.Contains(item.Layer))
                .ToList();

            var gaps = new List<TimelineGap>();

            if (mode is GapSearchMode.PerLayer or GapSearchMode.All)
                gaps.AddRange(FindPerLayer(items, maxGap));

            if (mode is GapSearchMode.AcrossLayers or GapSearchMode.All)
            {
                var across = FindAcrossLayers(items, maxGap).ToList();

                var covered = across.Select(gap => (gap.Frame, gap.EndFrame)).ToHashSet();
                gaps.RemoveAll(gap => covered.Contains((gap.Frame, gap.EndFrame)));

                gaps.AddRange(across);
            }

            return [.. gaps.OrderBy(gap => gap.Frame).ThenBy(gap => gap.Layer ?? int.MaxValue)];
        }

        private static IEnumerable<TimelineGap> FindPerLayer(IReadOnlyList<IItem> items, int maxGap)
        {
            foreach (var layer in items.GroupBy(item => item.Layer))
            {
                var ordered = layer.OrderBy(item => item.Frame).ToList();

                for (var i = 0; i + 1 < ordered.Count; i++)
                {
                    var before = ordered[i];
                    var after = ordered[i + 1];

                    var end = before.Frame + before.Length;
                    var length = after.Frame - end;

                    if (length > 0 && length <= maxGap)
                        yield return new TimelineGap(layer.Key, end, length, before, after);
                }
            }
        }

        private static IEnumerable<TimelineGap> FindAcrossLayers(IReadOnlyList<IItem> items, int maxGap)
        {
            var ordered = items.OrderBy(item => item.Frame).ToList();
            if (ordered.Count == 0)
                yield break;

            var reach = ordered[0].Frame + ordered[0].Length;
            var last = ordered[0];

            foreach (var item in ordered.Skip(1))
            {
                if (item.Frame > reach)
                {
                    var length = item.Frame - reach;
                    if (length <= maxGap)
                        yield return new TimelineGap(null, reach, length, last, item);

                    reach = item.Frame + item.Length;
                    last = item;
                    continue;
                }

                if (item.Frame + item.Length > reach)
                {
                    reach = item.Frame + item.Length;
                    last = item;
                }
            }
        }

        private static void Notify(string message) =>
            MessageBox.Show(message, Texts.GapCheckDisplayName, MessageBoxButton.OK);
    }
}
