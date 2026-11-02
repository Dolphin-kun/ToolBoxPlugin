using System.Windows;
using ToolBox.Commons;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.AnchorPoint
{
    internal static class AnchorPointCommand
    {
        public static string CommandName(PlacementPosition position) => $"AnchorPoint{position.Name}Command";

        public static bool CanApply(IToolBoxContext ctx) =>
            ctx.SelectedItems?.OfType<IVideoItem>().Any() == true && ctx.UndoRedoManager != null;

        public static void Apply(IToolBoxContext ctx, PlacementPosition position)
        {
            if (ctx.UndoRedoManager is not { } undoRedoManager)
                return;

            var items = ctx.SelectedItems?.OfType<IVideoItem>().ToList() ?? [];
            if (items.Count == 0)
                return;

            if (!CenterPointEffectHelper.IsAvailable && items.Any(item => !TextBasePoint.CanSet(item)))
            {
                MessageBox.Show(Texts.CenterPointUnavailable, Texts.AnchorPointDisplayName, MessageBoxButton.OK);
                return;
            }

            foreach (var item in items)
            {
                if (TextBasePoint.TrySet(item, position))
                    continue;

                if (position.IsCenter)
                    CenterPointEffectHelper.Remove(item);
                else
                    CenterPointEffectHelper.Apply(item, position.Horizontal, position.Vertical, keepPosition: true);
            }

            undoRedoManager.Record();
        }
    }
}
