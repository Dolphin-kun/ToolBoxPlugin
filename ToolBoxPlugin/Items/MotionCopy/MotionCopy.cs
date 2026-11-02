using ToolBox.Commons;
using ToolBox.Items.MotionCopy.Settings;
using ToolBox.Items.MotionCopy.Views;

namespace ToolBox.Items.MotionCopy
{
    public sealed class MotionCopy : DraggableItem
    {
        public override Type SettingsType => typeof(MotionCopySettings);
        public override Type ViewType => typeof(MotionCopyItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new(MotionCopyCommand.CopyCommandName, MotionCopyCommand.CanCopy, MotionCopyCommand.Copy,
                displayName: Texts.MotionCopyCommandName),
            new(MotionCopyCommand.PasteCommandName, MotionCopyCommand.CanPaste, MotionCopyCommand.Paste,
                displayName: Texts.MotionPasteCommandName),
        ];
    }
}
