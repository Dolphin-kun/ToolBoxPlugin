using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ToolBox.Commons;
using ToolBox.Items.TextSplitter.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items.TextSplitter
{
    internal static partial class TextSplitterCommand
    {
        [GeneratedRegex(@"^[a-zA-Z@]+")]
        private static partial Regex TagPrefixRegex();

        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex TagRegex();

        [GeneratedRegex(@"<r[a-z]+\s+([^, >]+)")]
        private static partial Regex RubyBaseRegex();

        [GeneratedRegex(@"<rt>.*?</rt>")]
        private static partial Regex RubyTextRegex();

        public static bool CanExecute(IReadOnlyList<IItem> selectedItems)
        {
            return selectedItems.Any(item => item switch
            {
                TextItem textItem => !string.IsNullOrEmpty(textItem.Text),
                VoiceItem voiceItem => !string.IsNullOrEmpty(voiceItem.Serif),
                _ => false,
            });
        }

        private static TextSplitterSettings GetSettings(IToolBoxContext ctx)
            => ctx.FindItemSettings<TextSplitterSettings>() ?? new TextSplitterSettings();

        public static void SplitText(IToolBoxContext ctx, IReadOnlyList<IItem> selectedItems, Timeline? timeline, UndoRedoManager? undoRedoManager)
        {
            if (timeline is null) return;
            if (undoRedoManager is null) return;
            if (selectedItems.Count == 0) return;

            var settings = GetSettings(ctx);

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
                        string tag = text[i..(end + 1)];
                        if (tag.StartsWith("<rb ") || tag.StartsWith("<ruby "))
                        {
                            result.Add(WrapWithTags(tag, activeTags));
                            i = end;
                            continue;
                        }

                        var match = TagPrefixRegex().Match(tag[1..^1]);
                        if (match.Success)
                        {
                            string prefix = match.Value;
                            activeTags.RemoveAll(t =>
                            {
                                var m = TagPrefixRegex().Match(t[1..^1]);
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

        private static List<List<string>> BuildLines(string text, TextSplitterSettings settings)
        {
            var lines = new List<List<string>>();

            if (settings.SplitMode == SplitMode.PerLine)
            {
                foreach (var line in text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None))
                    lines.Add(HasContent(line) ? [line] : []);
            }
            else
            {
                var currentLine = new List<string>();
                foreach (var element in SplitWithTags(text, settings.IsIgnoreTags))
                {
                    if (element is "\n" or "\r\n")
                    {
                        lines.Add(currentLine);
                        currentLine = [];
                    }
                    else if (HasContent(element))
                    {
                        currentLine.Add(element);
                    }
                }
                lines.Add(currentLine);
            }

            while (lines.Count > 0 && lines[^1].Count == 0)
                lines.RemoveAt(lines.Count - 1);

            return lines;
        }

        private static bool HasContent(string element) =>
            element.Contains("<rb ") || element.Contains("<ruby ")
            || !string.IsNullOrWhiteSpace(TagRegex().Replace(element, ""));

        private static void SplitSingleItem(IItem selectedItem, TextSplitterSettings settings, Timeline timeline)
        {
            string? textToSplit = selectedItem switch
            {
                TextItem textItem => textItem.Text,
                VoiceItem voiceItem => voiceItem.Serif,
                _ => null,
            };

            if (string.IsNullOrEmpty(textToSplit)) return;

            int startFrame = selectedItem.Frame;
            int startLayer = settings.IsDeleteOriginalItem
                ? selectedItem.Layer
                : selectedItem.Layer + 1;

            var lines = BuildLines(textToSplit, settings);

            var finalElements = new List<string>();
            var positionOffsets = new List<Point>();

            if (settings.IsKeepPosition)
            {
                LayOutLines(selectedItem, lines, finalElements, positionOffsets);
            }
            else
            {
                foreach (var element in lines.SelectMany(line => line))
                {
                    finalElements.Add(element);
                    positionOffsets.Add(new Point(0, 0));
                }
            }

            if (finalElements.Count == 0) return;

            int baseNewLength = 0;
            int remainder = 0;
            if (settings.IsKeepLength && settings.SplitDirection == SplitDirection.Horizontal)
            {
                baseNewLength = selectedItem.Length / finalElements.Count;
                remainder = selectedItem.Length % finalElements.Count;
            }

            int currentFrame = (settings.SplitDirection == SplitDirection.Horizontal && !settings.IsDeleteOriginalItem)
                ? startFrame + selectedItem.Length
                : startFrame;
            int targetStartFrame = currentFrame;

            var itemsToAdd = new List<IItem>();

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

                if (settings.IsKeepPosition && newItem is TextItem newTextItemPos)
                {
                    try
                    {
                        newTextItemPos.X.Values[0].Value += positionOffsets[i].X;
                        newTextItemPos.Y.Values[0].Value += positionOffsets[i].Y;
                    }
                    catch { }
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
            }

            timeline.TryAddItems(itemsArray, targetStartFrame, startLayer);
        }

        private static void LayOutLines(
            IItem selectedItem,
            List<List<string>> lines,
            List<string> finalElements,
            List<Point> positionOffsets)
        {
            string font = "Meiryo";
            double fontSize = 100;
            double letterSpacing = 0;
            string basePointName = "CenterCenter";
            double lineSpacingRatio = 100.0;

            if (selectedItem is TextItem textItem)
            {
                try { font = textItem.Font; } catch { }
                try { fontSize = textItem.FontSize.Values[0].Value; } catch { }
                try { letterSpacing = textItem.LetterSpacing2.Values[0].Value; } catch { }
                try { basePointName = textItem.BasePoint.ToString(); } catch { }
                try { lineSpacingRatio = textItem.LineHeight2.Values[0].Value; } catch { }
            }

            var typeface = new Typeface(font);
            double lineHeight = typeface.FontFamily.LineSpacing * fontSize * (lineSpacingRatio / 100.0);
            double totalHeight = lines.Count * lineHeight;

            double currentY = -totalHeight / 2;
            if (basePointName.Contains("Top")) currentY = 0;
            else if (basePointName.Contains("Bottom")) currentY = -totalHeight;

            foreach (var lineElements in lines)
            {
                var widths = lineElements
                    .Select(element => MeasureWidth(element, typeface, fontSize))
                    .ToList();

                double lineTotalWidth = widths.Sum() + letterSpacing * Math.Max(0, widths.Count - 1);

                double currentX = -lineTotalWidth / 2;
                if (basePointName.Contains("Left")) currentX = 0;
                else if (basePointName.Contains("Right")) currentX = -lineTotalWidth;

                for (int i = 0; i < lineElements.Count; i++)
                {
                    finalElements.Add(lineElements[i]);
                    positionOffsets.Add(new Point(currentX + widths[i] / 2, currentY + lineHeight / 2));
                    currentX += widths[i] + letterSpacing;
                }

                currentY += lineHeight;
            }
        }

        private static double MeasureWidth(string element, Typeface typeface, double fontSize)
        {
            string plainText = element;
            if (element.Contains("<rb ") || element.Contains("<ruby "))
            {
                var match = RubyBaseRegex().Match(element);
                if (match.Success) plainText = match.Groups[1].Value;
            }

            plainText = RubyTextRegex().Replace(plainText, "");
            plainText = TagRegex().Replace(plainText, "");
            if (string.IsNullOrEmpty(plainText)) plainText = " ";

            var formatted = new FormattedText(
                plainText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1.0);

            return formatted.WidthIncludingTrailingWhitespace;
        }
    }
}
