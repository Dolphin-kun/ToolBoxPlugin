using System.Windows;
using ToolBox.Commons;
using ToolBox.Items.Bookmarks.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Project;

namespace ToolBox.Items.Bookmarks
{
    internal static class BookmarkCommand
    {
        public static bool CanExecute(IToolBoxContext ctx) => ctx.Timeline != null;

        public static bool HasBookmarks(IToolBoxContext ctx) =>
            ctx.Timeline != null && GetSettings(ctx)?.Bookmarks.Count > 0;

        public static void Toggle(IToolBoxContext ctx)
        {
            if (ctx.Timeline is not { } timeline || GetSettings(ctx) is not { } settings)
                return;

            var frame = timeline.CurrentFrame;
            var existing = settings.Bookmarks.FirstOrDefault(entry => entry.Frame == frame);

            if (existing != null)
            {
                settings.Bookmarks.Remove(existing);
                return;
            }

            settings.Bookmarks.Add(new BookmarkEntry
            {
                Frame = frame,
                Name = string.Format(Texts.BookmarkDefaultName, settings.Bookmarks.Count + 1),
            });

            Sort(settings);
        }

        public static void MoveToNext(IToolBoxContext ctx) => Move(ctx, forward: true);

        public static void MoveToPrevious(IToolBoxContext ctx) => Move(ctx, forward: false);

        private static void Move(IToolBoxContext ctx, bool forward)
        {
            if (ctx.Timeline is not { } timeline || GetSettings(ctx) is not { } settings)
                return;

            var frames = settings.Bookmarks.Select(entry => entry.Frame).OrderBy(frame => frame).ToList();
            if (frames.Count == 0)
                return;

            var current = timeline.CurrentFrame;

            timeline.CurrentFrame = forward
                ? frames.FirstOrDefault(frame => frame > current, frames[0])
                : frames.LastOrDefault(frame => frame < current, frames[^1]);
        }

        public static void Clear(IToolBoxContext ctx)
        {
            if (GetSettings(ctx) is not { } settings || settings.Bookmarks.Count == 0)
                return;

            var answer = MessageBox.Show(
                Texts.BookmarkClearConfirm,
                Texts.BookmarkDisplayName,
                MessageBoxButton.OKCancel);

            if (answer == MessageBoxResult.OK)
                settings.Bookmarks.Clear();
        }

        public static void JumpTo(int frame)
        {
            if (CurrentTimeline is { } timeline)
                timeline.CurrentFrame = Math.Max(0, frame);
        }

        public static string FormatFrame(int frame)
        {
            if (CurrentTimeline?.VideoInfo is not { } info || info.FPS <= 0)
                return string.Format(Texts.BookmarkFrameFormat, frame);

            var time = info.GetTimeFrom(frame);
            var frames = frame - (int)(time.TotalSeconds * info.FPS);

            return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}.{Math.Max(0, frames):00}";
        }

        private static void Sort(BookmarkSettings settings)
        {
            var ordered = settings.Bookmarks.OrderBy(entry => entry.Frame).ToList();

            settings.Bookmarks.Clear();
            foreach (var entry in ordered)
                settings.Bookmarks.Add(entry);
        }

        private static Timeline? CurrentTimeline => (ToolBoxViewModel.Instance as IToolBoxContext)?.Timeline;

        private static BookmarkSettings? GetSettings(IToolBoxContext ctx) => ctx.FindItemSettings<BookmarkSettings>();
    }
}
