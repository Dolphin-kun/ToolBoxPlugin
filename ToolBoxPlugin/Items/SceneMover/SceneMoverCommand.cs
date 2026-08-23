using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ToolBox.Commons;
using ToolBox.Items.SceneMover.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Items.SceneMover
{
    public sealed record SceneMoveTarget(SceneMoverSettings? Settings, Timeline Target);

    internal static class SceneMoverCommand
    {
        private static string Caption => Texts.SceneMoverDisplayName;
        private const int MaxSceneSteps = 20;

        public static string? DescribeBlock(IToolBoxContext ctx)
        {
            if (ctx.Timeline is null || ctx.UndoRedoManager is null)
                return Texts.SceneMoverNoTimeline;

            if (ctx.SelectedItems is not { Count: > 0 })
                return Texts.SceneMoverNoSelection;

            return null;
        }

        public static bool CanMove(IToolBoxContext ctx) => DescribeBlock(ctx) == null;

        public static bool CanMoveToNewScene(IToolBoxContext ctx) => CanMove(ctx) && ctx.Scenes != null;

        public static IReadOnlyList<Timeline> GetOtherScenes(IToolBoxContext ctx)
        {
            if (ctx.Scenes is not { } scenes)
                return [];

            return [.. scenes.Timelines.Where(timeline => !ReferenceEquals(timeline, ctx.Timeline))];
        }

        public static void MoveToNewScene(IToolBoxContext ctx, SceneMoverSettings? settings)
        {
            if (Report(DescribeBlock(ctx)))
                return;

            if (ctx.Scenes is not { } scenes)
            {
                Report(Texts.SceneMoverNoScenes);
                return;
            }

            var target = CreateScene(scenes, ctx.Timeline!);
            if (target == null)
                return;

            if (!Move(ctx, settings, target))
            {
                try
                {
                    scenes.DeleteScene(target);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to remove the unused scene: {ex}");
                }
            }
        }

        public static void MoveToScene(IToolBoxContext ctx, SceneMoverSettings? settings, Timeline target)
        {
            if (Report(DescribeBlock(ctx)))
                return;

            Move(ctx, settings, target);
        }

        private static bool Move(IToolBoxContext ctx, SceneMoverSettings? settings, Timeline target)
        {
            var source = ctx.Timeline;
            var undoRedoManager = ctx.UndoRedoManager;
            var items = ctx.SelectedItems?.ToList();

            if (source is null || undoRedoManager is null || items is null || items.Count == 0)
                return false;

            if (ReferenceEquals(source, target))
            {
                Report(Texts.SceneMoverAlreadyHere);
                return false;
            }

            try
            {
                var clones = items.Select(item => item.GetClone()).ToList();
                var frame = settings?.AlignToSceneStart == true ? 0 : clones.Min(clone => clone.Frame);
                var layer = clones.Min(clone => clone.Layer);

                if (!TimelineHelper.AddItems(target, clones, frame, layer))
                {
                    Report(Texts.SceneMoverAddFailed);
                    return false;
                }

                if (settings?.DuplicateInsteadOfMove != true)
                {
                    source.DeleteItems(items);
                    source.ResolveAllItemsCollision();
                    source.RefreshTimelineLengthAndMaxLayer();
                }

                undoRedoManager.Record();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to move items to another scene: {ex}");
                Report($"{Texts.SceneMoverMoveFailed}\n{ex.Message}");
                return false;
            }

            if (settings?.SwitchToTargetScene != false)
                SelectScene(ctx, target, MaxSceneSteps);

            return true;
        }

        private static Timeline? CreateScene(Scenes scenes, Timeline source)
        {
            try
            {
                var videoInfo = source.VideoInfo;
                var timeline = new Timeline(
                    CreateSceneName(scenes),
                    videoInfo.Width,
                    videoInfo.Height,
                    videoInfo.FPS,
                    videoInfo.Hz,
                    source.VerticalLine.GetClone(),
                    null);

                scenes.AddScene(timeline);
                return timeline;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create a scene: {ex}");
                Report($"{Texts.SceneMoverCreateFailed}\n{ex.Message}");
                return null;
            }
        }

        private static string CreateSceneName(Scenes scenes)
        {
            var existing = scenes.Timelines.Select(timeline => timeline.Name).ToHashSet(StringComparer.Ordinal);

            for (var index = 1; index < 1000; index++)
            {
                var name = string.Format(Texts.SceneMoverNewSceneName, index);
                if (existing.Add(name))
                    return name;
            }

            return Texts.SceneMoverSceneWord;
        }

        private static void SelectScene(IToolBoxContext ctx, Timeline target, int remaining)
        {
            if (remaining <= 0 || ReferenceEquals(ctx.Timeline, target))
                return;

            if (ctx.Scenes is not { } scenes)
                return;

            var timelines = scenes.Timelines;
            var current = ctx.Timeline;
            var currentIndex = current == null ? -1 : timelines.IndexOf(current);
            var targetIndex = timelines.IndexOf(target);

            if (currentIndex < 0 || targetIndex < 0 || currentIndex == targetIndex)
                return;

            var forward = targetIndex > currentIndex;
            if (!RunCommand(forward ? CommandType.SelectNextScene : CommandType.SelectPreviousScene))
                return;

            Application.Current?.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                () =>
                {
                    if (ReferenceEquals(ctx.Timeline, current))
                        return;

                    SelectScene(ctx, target, remaining - 1);
                });
        }

        private static bool RunCommand(CommandType commandType)
        {
            try
            {
                if (CommandSettings.Default[commandType] is not RoutedCommand routed)
                    return false;

                var window = Application.Current?.MainWindow;
                if (!routed.CanExecute(null, window))
                    return false;

                routed.Execute(null, window);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to run {commandType}: {ex}");
                return false;
            }
        }

        private static bool Report(string? message)
        {
            if (message == null)
                return false;

            MessageBox.Show(message, Caption, MessageBoxButton.OK);
            return true;
        }
    }
}
