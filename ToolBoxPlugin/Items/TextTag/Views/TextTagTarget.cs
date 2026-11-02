using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.TextTag.Views
{
    public sealed class TextTagTarget : Bindable
    {
        public IItem Item { get; }

        private string text;
        public string Text
        {
            get => text;
            set => Set(ref text, value ?? string.Empty, etcChangedPropertyNames: nameof(Caption));
        }

        public string Original { get; }

        public string Caption => Describe(Text);

        public bool IsChanged => Text != Original;

        private TextTagTarget(IItem item, string text)
        {
            Item = item;
            Original = text;
            this.text = text;
        }

        public void Push() => Write(Text);

        public void Restore() => Write(Original);

        private void Write(string value)
        {
            switch (Item)
            {
                case VoiceItem voice when voice.Serif != value:
                    voice.Serif = value;
                    break;
                case TextItem textItem when textItem.Text != value:
                    textItem.Text = value;
                    break;
            }
        }

        public static TextTagTarget? From(IItem item) => item switch
        {
            VoiceItem voice => new TextTagTarget(voice, voice.Serif ?? string.Empty),
            TextItem text => new TextTagTarget(text, text.Text ?? string.Empty),
            _ => null,
        };

        private static string Describe(string? value)
        {
            var head = (value ?? string.Empty).Replace("\n", " ").Trim();
            if (head.Length > 30)
                head = head[..30] + "…";

            return head.Length == 0 ? Texts.TextTagEmptyTarget : head;
        }
    }
}
