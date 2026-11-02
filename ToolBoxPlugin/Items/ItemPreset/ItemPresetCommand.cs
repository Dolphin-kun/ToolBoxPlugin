using System.Windows;
using ToolBox.Commons;
using ToolBox.Items.ItemPreset.Settings;
using ToolBox.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.ItemPreset
{
    internal static class ItemPresetCommand
    {
        private static string Caption => Texts.ItemStockTitle;

        public static bool CanAdd(IToolBoxContext ctx, ItemPresetSettings settings) =>
            ctx.Timeline != null && ctx.UndoRedoManager != null && settings.HasStock;

        public static bool CanSave(IToolBoxContext ctx) => ctx.SelectedItems?.Count > 0;

        public static void SaveAsNewStock(IToolBoxContext ctx, ItemPresetSettings settings)
        {
            var entry = CreateStock(ctx);
            if (entry == null)
                return;

            settings.StockId = entry.Id;
            ToolBoxViewModel.RefreshAllCommands();
        }

        public static ItemStockEntry? CreateStock(IToolBoxContext ctx)
        {
            var items = ctx.SelectedItems?.ToList();
            if (items is null || items.Count == 0)
                return null;

            var entry = ItemStockStore.Create(items);
            if (entry == null)
            {
                MessageBox.Show(
                    Texts.ItemStockSaveFailed,
                    Caption,
                    MessageBoxButton.OK);
                return null;
            }

            ItemStockStore.Entries.Add(entry);
            return entry;
        }

        public static void Overwrite(IToolBoxContext ctx, ItemPresetSettings settings)
        {
            var items = ctx.SelectedItems?.ToList();
            if (items is null || items.Count == 0 || settings.Stock is not { } entry)
                return;

            if (!ItemStockStore.Overwrite(entry, items))
            {
                MessageBox.Show(
                    Texts.ItemStockSaveFailed,
                    Caption,
                    MessageBoxButton.OK);
                return;
            }

            ToolBoxViewModel.RefreshAllCommands();
        }

        public static void Add(IToolBoxContext ctx, ItemPresetSettings settings)
        {
            var timeline = ctx.Timeline;
            var undoRedoManager = ctx.UndoRedoManager;

            if (timeline is null || undoRedoManager is null || settings.Stock is not { } entry)
                return;

            var items = ItemStockStore.Load(entry);
            if (items is null || items.Count == 0)
            {
                MessageBox.Show(
                    string.Format(Texts.ItemPresetLoadFailed, entry.DisplayLabel),
                    Caption,
                    MessageBoxButton.OK);
                return;
            }

            var frame = settings.UseCurrentFrame ? Math.Max(0, timeline.CurrentFrame) : items.Min(item => item.Frame);
            var layer = settings.UseCurrentLayer ? TimelineHelper.GetCurrentLayer(timeline) : items.Min(item => item.Layer);

            if (!TimelineHelper.AddItems(timeline, items, frame, layer))
                return;

            if (settings.SelectAfterAdd)
                timeline.SelectItems(items);

            undoRedoManager.Record();
        }
    }
}
