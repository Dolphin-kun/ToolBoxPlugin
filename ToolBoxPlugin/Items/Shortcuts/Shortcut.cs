using ToolBox.Commons;
using ToolBox.Items.Shortcuts.Settings;
using ToolBox.Items.Shortcuts.Views;

namespace ToolBox.Items.Shortcuts
{
    public sealed class Shortcut : DraggableItem
    {
        public override Type SettingsType => typeof(ShortcutSettings);
        public override Type ViewType => typeof(ShortcutItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(ShortcutSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("ShortcutCommand",
                (ctx, param) => true,
                (ctx, param) =>
                {
                    if (param is not ShortcutSettings settings) return;

                    ShortcutDefinitions.Find(settings.CommandType)?.Execute(ctx);
                },
                displayName: Texts.ShortcutRunCommandName,
                requiresItemParameter: true)
        ];
    }
}
