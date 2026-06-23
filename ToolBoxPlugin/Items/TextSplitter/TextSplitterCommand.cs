using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToolBox.Items.TextSplitter.Settings;
using ToolBox.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items.TextSplitter
{
    internal static class TextSplitterCommand
    {
        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any(item =>
            {
                if (item is TextItem textItem)
                {
                    return !string.IsNullOrEmpty(textItem.Text);
                }
                if (item is VoiceItem voiceItem)
                {
                    return !string.IsNullOrEmpty(voiceItem.Serif);
                }
                return false;
            });
        }

        private static TextSplitterSettings GetSettings()
            => ToolBoxSettings.Default.Items
                .OfType<TextSplitterSettings>()
                .FirstOrDefault() ?? new TextSplitterSettings();

        public static void SplitText(IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null) return;
            if (undoRedoManager is null) return;
            if (selectedItems.Count == 0) return;

            var settings = GetSettings();

            var itemsToSplit = selectedItems
                .Where(item => item is TextItem || item is VoiceItem)
                .ToList();

            if (itemsToSplit.Count == 0) return;

            foreach (var item in itemsToSplit)
            {
                SplitSingleItem(item, settings, timeline);
            }

            undoRedoManager.Record();
        }

        private static void SplitSingleItem(IItem selectedItem, TextSplitterSettings settings, Timeline timeline)
        {
            string? textToSplit = null;
            if (selectedItem is TextItem textItem)
            {
                textToSplit = textItem.Text;
            }
            else if (selectedItem is VoiceItem voiceItem)
            {
                textToSplit = voiceItem.Serif;
            }

            if (string.IsNullOrEmpty(textToSplit)) return;

            int startFrame = selectedItem.Frame;
            int startLayer = settings.IsDeleteOriginalItem
                ? selectedItem.Layer
                : selectedItem.Layer + 1;

            var itemsToAdd = new List<IItem>();
            IEnumerable<string> textElements;

            if (settings.SplitMode == SplitMode.PerLine)
            {
                textElements = textToSplit.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
            else
            {
                var elements = new List<string>();
                var enumerator = StringInfo.GetTextElementEnumerator(textToSplit);
                while (enumerator.MoveNext())
                {
                    elements.Add(enumerator.GetTextElement());
                }
                textElements = elements;
            }

            var validTextElements = textElements.Where(te => !string.IsNullOrWhiteSpace(te)).ToList();
            int totalSplitItems = validTextElements.Count;

            if (totalSplitItems == 0) return;

            int baseNewLength = 0;
            int remainder = 0;
            if (settings.IsKeepLength && settings.SplitDirection == SplitDirection.Horizontal && totalSplitItems > 0)
            {
                baseNewLength = selectedItem.Length / totalSplitItems;
                remainder = selectedItem.Length % totalSplitItems;
            }

            int i = 0;
            int currentFrame = (settings.SplitDirection == SplitDirection.Horizontal && !settings.IsDeleteOriginalItem)
                ? startFrame + selectedItem.Length
                : startFrame;
            int targetStartFrame = currentFrame;

            foreach (string textElement in validTextElements)
            {
                var newItem = selectedItem.GetClone();
                if (newItem is TextItem newTextItem)
                {
                    newTextItem.Text = textElement;
                }
                else if (newItem is VoiceItem newVoiceItem)
                {
                    newVoiceItem.Serif = textElement;
                }

                if (settings.SplitDirection == SplitDirection.Vertical)
                {
                    newItem.Frame = startFrame + (i * settings.FrameOffset);
                    newItem.Layer = startLayer + i;
                }
                else if (settings.SplitDirection == SplitDirection.Horizontal)
                {
                    newItem.Frame = currentFrame;
                    newItem.Layer = startLayer;

                    int itemLength;
                    if (settings.IsKeepLength)
                    {
                        itemLength = baseNewLength + (i < remainder ? 1 : 0);
                        newItem.Length = itemLength;
                    }
                    else
                    {
                        itemLength = newItem.Length;
                    }

                    currentFrame += itemLength;
                }

                itemsToAdd.Add(newItem);
                i++;
            }
            if (itemsToAdd.Count == 0) return;
            IItem[] itemsArray = itemsToAdd.ToArray();

            if (settings.IsDeleteOriginalItem)
            {
                timeline.DeleteItems(new[] { selectedItem });
                timeline.TryAddItems(itemsArray, targetStartFrame, startLayer);
            }
            else
            {
                timeline.TryAddItems(itemsArray, targetStartFrame, startLayer);
            }
        }
    }
}
