using ToolBox.Commons;
using ToolBox.Items.KeyFrameTools.Settings;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.KeyFrameTools
{
    internal static class KeyFrameToolsCommand
    {
        public static bool CanEdit(IToolBoxContext ctx) =>
            ctx.SelectedItems?.Count > 0 && ctx.UndoRedoManager != null;

        public static void Reverse(IToolBoxContext ctx)
        {
            if (ctx.SelectedItems is not { Count: > 0 } items || ctx.UndoRedoManager is not { } undoRedoManager)
                return;

            if (ctx.FindItemSettings<KeyFrameToolsSettings>()?.CopyReversed == true)
            {
                foreach (var item in items)
                    AnimationHelper.MirrorKeyFrames(item);
            }
            else
            {
                foreach (var item in items)
                    Reverse(item);
            }

            undoRedoManager.Record();
        }

        private static void Reverse(IItem item)
        {
            AnimationHelper.ReverseKeyFrames(item.KeyFrames, item.Length);

            foreach (var animation in AnimationHelper.GetAnimations(item))
                AnimationHelper.Reverse(animation, item.Length);
        }

        public static void Shift(IToolBoxContext ctx, int direction)
        {
            if (ctx.SelectedItems is not { Count: > 0 } items || ctx.UndoRedoManager is not { } undoRedoManager)
                return;

            var frames = (ctx.FindItemSettings<KeyFrameToolsSettings>()?.ShiftFrames ?? 5) * direction;

            var changed = false;
            foreach (var item in items)
                changed |= AnimationHelper.ShiftKeyFrames(item.KeyFrames, item.Length, frames);

            if (changed)
                undoRedoManager.Record();
        }
    }
}
