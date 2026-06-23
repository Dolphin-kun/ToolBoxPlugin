using ToolBox.Items.Recording.Settings;
using ToolBox.Items.Recording.ViewModels;
using ToolBox.Items.Recording.Views;
using ToolBox.Settings;

namespace ToolBox.Items.Recording
{
    public sealed class Recording : DraggableItem
    {
        public override Type SettingsType => typeof(RecordingSettings);
        public override Type ViewType => typeof(RecordingItem);
        public override Type? ViewModelType => typeof(RecordingViewModel);
        public override Type? SettingsViewType => typeof(RecordingSettingsPanel);
        public override Type? SettingsViewModelType => typeof(RecordingSettingsViewModel);
        public override string IconPath => "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9Z";

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("ToggleRecordingCommand",
                ctx => RecordingCommand.CanExecute(ctx.Timeline, ctx.UndoRedoManager),
                ctx => RecordingCommand.ToggleRecording(
                    ctx.Timeline,
                    ctx.UndoRedoManager,
                    ToolBoxSettings.Default.Items.OfType<RecordingSettings>().FirstOrDefault()?.RecordingDeviceIndex ?? 0)),
        ];
    }
}
