using ToolBox.Commons;
using ToolBox.Items.Easing.Settings;
using ToolBox.Items.Easing.Views;

namespace ToolBox.Items.Easing
{
    public sealed class Easing : DraggableItem
    {
        public override Type SettingsType => typeof(EasingSettings);
        public override Type ViewType => typeof(EasingItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("ApplyEasingCommand",
                EasingCommand.CanApply,
                EasingCommand.Apply,
                displayName: Texts.EasingApplyCommandName),
        ];
    }
}
