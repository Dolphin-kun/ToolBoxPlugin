using System.Windows;
using ToolBox.Commons;
using ToolBox.Items.TextTag.Views;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.TextTag
{
    internal static class TextTagCommand
    {
        public static bool CanExecute(IToolBoxContext ctx) =>
            ctx.Timeline != null && GetTargets(ctx).Count > 0;

        public static void Open(IToolBoxContext ctx)
        {
            var targets = GetTargets(ctx);
            if (targets.Count == 0)
            {
                MessageBox.Show(Texts.TextTagNoTarget, Texts.TextTagDisplayName, MessageBoxButton.OK);
                return;
            }

            var window = new TextTagWindow(targets)
            {
                Owner = Application.Current?.MainWindow,
            };

            if (window.ShowDialog() != true)
                return;

            if (window.ChangedTargets.Count == 0)
                return;

            ctx.UndoRedoManager?.Record();
        }

        private static IReadOnlyList<IItem> GetTargets(IToolBoxContext ctx)
        {
            var selected = ctx.SelectedItems?.Where(IsTextItem).ToList() ?? [];
            if (selected.Count > 0)
                return selected;

            return ctx.Timeline?.Items.Where(IsTextItem).ToList() ?? [];
        }

        private static bool IsTextItem(IItem item) => item is VoiceItem or TextItem;
    }
}
