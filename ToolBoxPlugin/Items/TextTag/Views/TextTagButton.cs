using ToolBox.Items.TextTag.Tags;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Items.TextTag.Views
{
    public sealed class TextTagButton : Bindable
    {
        public TextTagDefinition Definition { get; }

        public TextTagParameters Parameters { get; } = new();

        public string Label => Definition.Label;

        public string IconPath => Definition.IconPath;

        public string Sample => Definition.Sample;

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            set => Set(ref isEnabled, value);
        }

        public TextTagButton(TextTagDefinition definition)
        {
            Definition = definition;
            Parameters.UseFontFamily = definition.Kind == TextTagKind.Font;
        }
    }
}
