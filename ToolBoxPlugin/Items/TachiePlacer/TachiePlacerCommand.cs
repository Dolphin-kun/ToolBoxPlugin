using System.Collections.Generic;
using System.Linq;
using ToolBox.Items.TachiePlacer.Settings;
using ToolBox.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items.TachiePlacer
{
    internal static class TachiePlacerCommand
    {
        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any(item => item is VoiceItem voiceItem && voiceItem.Character != null);
        }

        private static TachiePlacerSettings GetSettings()
            => ToolBoxSettings.Default.Items
                .OfType<TachiePlacerSettings>()
                .FirstOrDefault() ?? new TachiePlacerSettings();

        public static void AddTachie(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null || undoRedoManager is null || selectedItems.Count == 0) return;

            var settings = GetSettings();

            var voices = selectedItems
                .OfType<VoiceItem>()
                .Where(v => v.Character != null)
                .ToList();

            if (voices.Count == 0) return;

            var itemsToAdd = new List<IItem>();

            foreach (var voiceItem in voices)
            {
                // Create new TachieItem using the decompiler constructor: public TachieItem(Character character)
                var tachieItem = new TachieItem(voiceItem.Character)
                {
                    Frame = voiceItem.Frame,
                    Length = voiceItem.Length
                };

                // Determine target layer based on settings
                if (settings.Placement == TachiePlacement.Below)
                {
                    tachieItem.Layer = voiceItem.Layer + 1;
                }
                else // Above
                {
                    tachieItem.Layer = System.Math.Max(0, voiceItem.Layer - 1);
                }

                itemsToAdd.Add(tachieItem);
            }

            if (itemsToAdd.Count == 0) return;

            // Add each item individually to its computed layer and frame
            foreach (var item in itemsToAdd)
            {
                timeline.TryAddItems(new[] { item }, item.Frame, item.Layer);
            }

            undoRedoManager.Record();
        }
    }
}
