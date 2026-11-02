using System.Windows.Controls;

namespace ToolBox.Items.TextTag.Tags
{
    public static class TextTagEditor
    {
        public static void Apply(TextBox box, TextTagDefinition definition, TextTagParameters parameters)
        {
            switch (definition.Apply)
            {
                case TextTagApply.Wrap:
                    Wrap(box, definition.Kind, parameters);
                    break;
                case TextTagApply.Replace:
                    Replace(box, definition.Kind, parameters);
                    break;
                default:
                    Insert(box, definition.Kind, parameters);
                    break;
            }
        }

        private static void Wrap(TextBox box, TextTagKind kind, TextTagParameters parameters)
        {
            if (box.SelectionLength == 0)
                return;

            var (start, length) = SnapToTagBoundary(box.Text, box.SelectionStart, box.SelectionLength);

            var body = box.Text.Substring(start, length);
            var open = TextTagBuilder.BuildOpen(kind, parameters, body);
            var close = TextTagBuilder.BuildClose(kind, parameters);

            box.Text = box.Text.Remove(start, length).Insert(start, open + body + close);
            box.Select(start + open.Length, length);
        }

        private static void Replace(TextBox box, TextTagKind kind, TextTagParameters parameters)
        {
            if (box.SelectionLength == 0)
                return;

            var (start, length) = SnapToTagBoundary(box.Text, box.SelectionStart, box.SelectionLength);

            var body = box.Text.Substring(start, length);
            var tag = TextTagBuilder.BuildOpen(kind, parameters, body);

            box.Text = box.Text.Remove(start, length).Insert(start, tag);
            box.Select(start, tag.Length);
        }

        public static (int start, int length) SnapToTagBoundary(string text, int start, int length)
        {
            var end = MoveOutOfTag(text, start + length);
            start = MoveToTagStart(text, start);

            return (start, end - start);
        }

        public static int MoveToTagStart(string text, int index)
        {
            index = Math.Clamp(index, 0, text.Length);

            var open = text.LastIndexOf('<', Math.Max(0, index - 1));
            if (open < 0 || open >= index || IsEscaped(text, open))
                return index;

            var close = text.IndexOf('>', open);
            return close >= index ? open : index;
        }

        private static void Insert(TextBox box, TextTagKind kind, TextTagParameters parameters)
        {
            var at = MoveOutOfTag(box.Text, box.SelectionLength > 0
                ? box.SelectionStart + box.SelectionLength
                : box.CaretIndex);

            var tag = TextTagBuilder.BuildOpen(kind, parameters, string.Empty);

            box.Text = box.Text.Insert(at, tag);
            box.CaretIndex = at + tag.Length;
            box.Focus();
        }

        public static int MoveOutOfTag(string text, int index)
        {
            index = Math.Clamp(index, 0, text.Length);

            var open = text.LastIndexOf('<', Math.Max(0, index - 1));
            if (open < 0 || IsEscaped(text, open))
                return index;

            var close = text.IndexOf('>', open);
            if (close < 0 || close < index - 1)
                return index;

            return close + 1;
        }

        private static bool IsEscaped(string text, int index)
        {
            var backslashes = 0;
            for (var i = index - 1; i >= 0 && text[i] == '\\'; i--)
                backslashes++;

            return backslashes % 2 == 1;
        }
    }
}
