using ToolBox.Commons;
using ToolBox.Items.KeyFrameTools.Settings;
using ToolBox.Items.KeyFrameTools.Views;

namespace ToolBox.Items.KeyFrameTools
{
    public sealed class KeyFrameTools : DraggableItem
    {
        public override Type SettingsType => typeof(KeyFrameToolsSettings);
        public override Type ViewType => typeof(KeyFrameToolsItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("KeyFrameShiftEarlierCommand",
                KeyFrameToolsCommand.CanEdit,
                ctx => KeyFrameToolsCommand.Shift(ctx, -1),
                displayName: Texts.KeyFrameShiftEarlierCommandName),
            new("KeyFrameReverseCommand",
                KeyFrameToolsCommand.CanEdit,
                KeyFrameToolsCommand.Reverse,
                displayName: Texts.KeyFrameReverseCommandName),
            new("KeyFrameShiftLaterCommand",
                KeyFrameToolsCommand.CanEdit,
                ctx => KeyFrameToolsCommand.Shift(ctx, 1),
                displayName: Texts.KeyFrameShiftLaterCommandName),
        ];
    }
}
