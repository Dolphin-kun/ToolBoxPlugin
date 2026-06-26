using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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



        private static List<string> SplitWithTags(string text, bool ignoreTags)
        {
            if (!ignoreTags)
            {
                var elements = new List<string>();
                var enumerator = StringInfo.GetTextElementEnumerator(text);
                while (enumerator.MoveNext()) elements.Add(enumerator.GetTextElement());
                return elements;
            }

            var result = new List<string>();
            var activeTags = new List<string>();

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '<')
                {
                    int end = text.IndexOf('>', i);
                    if (end != -1)
                    {
                        string tag = text.Substring(i, end - i + 1);
                        if (tag.StartsWith("<rb ") || tag.StartsWith("<ruby "))
                        {
                            result.Add(WrapWithTags(tag, activeTags));
                            i = end;
                            continue;
                        }

                        string inner = tag.Substring(1, tag.Length - 2);
                        var match = System.Text.RegularExpressions.Regex.Match(inner, @"^[a-zA-Z@]+");
                        if (match.Success)
                        {
                            string prefix = match.Value;
                            activeTags.RemoveAll(t => 
                            {
                                var m = System.Text.RegularExpressions.Regex.Match(t.Substring(1, t.Length - 2), @"^[a-zA-Z@]+");
                                return m.Success && m.Value == prefix;
                            });

                            if (tag != "<" + prefix + ">")
                            {
                                activeTags.Add(tag);
                            }
                        }
                        else
                        {
                            activeTags.Add(tag);
                        }
                        i = end;
                        continue;
                    }
                }

                string charStr = StringInfo.GetNextTextElement(text, i);
                result.Add(WrapWithTags(charStr, activeTags));
                i += charStr.Length - 1;
            }

            return result;
        }

        private static string WrapWithTags(string text, List<string> activeTags)
        {
            if (text == "\n" || text == "\r\n") return text;
            string wrapped = text;
            foreach (var tag in activeTags) wrapped = tag + wrapped;
            return wrapped;
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
                textElements = textToSplit.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            }
            else
            {
                textElements = SplitWithTags(textToSplit, settings.IsIgnoreTags);
            }

            var validTextElements = textElements.Where(te => te == "\n" || te == "\r\n" || te.Contains("<rb ") || te.Contains("<ruby ") || !string.IsNullOrWhiteSpace(System.Text.RegularExpressions.Regex.Replace(te, @"<[^>]+>", ""))).ToList();
            int totalSplitItems = validTextElements.Count;

            if (totalSplitItems == 0) return;

            int baseNewLength = 0;
            int remainder = 0;
            if (settings.IsKeepLength && settings.SplitDirection == SplitDirection.Horizontal && totalSplitItems > 0)
            {
                baseNewLength = selectedItem.Length / totalSplitItems;
                remainder = selectedItem.Length % totalSplitItems;
            }

            int currentFrame = (settings.SplitDirection == SplitDirection.Horizontal && !settings.IsDeleteOriginalItem)
                ? startFrame + selectedItem.Length
                : startFrame;
            int targetStartFrame = currentFrame;

            var finalElements = new List<string>();
            var positionOffsets = new List<Point>();

            if (settings.IsKeepPosition)
            {
                string font = "Meiryo";
                double fontSize = 100;
                double letterSpacing = 0;
                string basePointName = "CenterCenter";

                if (selectedItem is TextItem tItemFont)
                {
                    try { font = tItemFont.Font; } catch { }
                    try { fontSize = tItemFont.FontSize.Values[0].Value; } catch { }
                    try { letterSpacing = tItemFont.LetterSpacing2.Values[0].Value; } catch { }
                    try { basePointName = tItemFont.BasePoint.ToString(); } catch { }
                }

                var typeface = new Typeface(font);
                
                var lines = new List<List<string>>();
                var currentLine = new List<string>();
                foreach (var te in validTextElements)
                {
                    if (te == "\n" || te == "\r\n")
                    {
                        lines.Add(currentLine);
                        currentLine = new List<string>();
                    }
                    else
                    {
                        currentLine.Add(te);
                    }
                }
                lines.Add(currentLine);

                double lineSpacingRatio = 100.0;
                if (selectedItem is TextItem tItemSpacing)
                {
                    try { lineSpacingRatio = tItemSpacing.LineHeight2.Values[0].Value; } catch { }
                }

                double baseLineHeight = typeface.FontFamily.LineSpacing * fontSize;
                double lineHeight = baseLineHeight * (lineSpacingRatio / 100.0);
                double totalHeight = lines.Count * lineHeight;
                double startY = -totalHeight / 2;
                if (basePointName.Contains("Top")) startY = 0;
                else if (basePointName.Contains("Bottom")) startY = -totalHeight;

                double currentY = startY;

                foreach (var lineElements in lines)
                {
                    double lineTotalWidth = 0;
                    var lineCharSizes = new List<Size>();
                    
                    foreach (var te in lineElements)
                    {
                        string plainText = te;
                        if (te.Contains("<rb ") || te.Contains("<ruby "))
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(te, @"<r[a-z]+\s+([^, >]+)");
                            if (match.Success) plainText = match.Groups[1].Value;
                        }
                        
                        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"<rt>.*?</rt>", "");
                        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"<[^>]+>", "");
                        if (string.IsNullOrEmpty(plainText)) plainText = " ";

                        var ft = new FormattedText(
                            plainText,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            typeface,
                            fontSize,
                            Brushes.Black,
                            new NumberSubstitution(),
                            1.0);

                        double w = ft.WidthIncludingTrailingWhitespace;
                        lineCharSizes.Add(new Size(w, ft.Height));
                        lineTotalWidth += w + letterSpacing;
                    }
                    if (lineCharSizes.Count > 0) lineTotalWidth -= letterSpacing;

                    double currentX = -lineTotalWidth / 2;
                    if (basePointName.Contains("Left")) currentX = 0;
                    else if (basePointName.Contains("Right")) currentX = -lineTotalWidth;

                    for (int j = 0; j < lineElements.Count; j++)
                    {
                        double w = lineCharSizes[j].Width;
                        double charCenterX = currentX + w / 2;
                        double charCenterY = currentY + lineHeight / 2;

                        finalElements.Add(lineElements[j]);
                        positionOffsets.Add(new Point(charCenterX, charCenterY));
                        
                        currentX += w + letterSpacing;
                    }
                    currentY += lineHeight;
                }
            }
            else
            {
                foreach (var te in validTextElements)
                {
                    if (te != "\n" && te != "\r\n")
                    {
                        finalElements.Add(te);
                        positionOffsets.Add(new Point(0, 0));
                    }
                }
            }

            for (int i = 0; i < finalElements.Count; i++)
            {
                var textElement = finalElements[i];
                var newItem = selectedItem.GetClone();
                if (newItem is TextItem newTextItem)
                {
                    newTextItem.Text = textElement;
                }
                else if (newItem is VoiceItem newVoiceItem)
                {
                    newVoiceItem.Serif = textElement;
                }

                if (settings.IsKeepPosition)
                {
                    if (newItem is TextItem newTextItemPos)
                    {
                        try
                        {
                            newTextItemPos.X.Values[0].Value += positionOffsets[i].X;
                            newTextItemPos.Y.Values[0].Value += positionOffsets[i].Y;
                        }
                        catch { }
                    }
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
            }
            if (itemsToAdd.Count == 0) return;
            IItem[] itemsArray = [.. itemsToAdd];

            if (settings.IsDeleteOriginalItem)
            {
                timeline.DeleteItems([selectedItem]);
                timeline.TryAddItems(itemsArray, targetStartFrame, startLayer);
            }
            else
            {
                timeline.TryAddItems(itemsArray, targetStartFrame, startLayer);
            }
        }
    }
}
