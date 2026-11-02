using ToolBox.Commons;
using ToolBox.Items.TextTag.Settings;
using ToolBox.Items.TextTag.Views;

namespace ToolBox.Items.TextTag
{
    public sealed class TextTag : DraggableItem
    {
        public override Type SettingsType => typeof(TextTagSettings);
        public override Type ViewType => typeof(TextTagItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("TextTagCommand",
                TextTagCommand.CanExecute,
                TextTagCommand.Open,
                displayName: Texts.TextTagDisplayName),
        ];
    }
}
