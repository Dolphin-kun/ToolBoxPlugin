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

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("RecordingItemCommand",
                ctx => RecordingCommand.CanExecute(ctx.Timeline, ctx.UndoRedoManager),
                ctx => RecordingCommand.ToggleRecording(
                    ctx.Timeline,
                    ctx.UndoRedoManager,
                    ToolBoxSettings.Default.Items.OfType<RecordingSettings>().FirstOrDefault()?.RecordingDeviceIndex ?? 0)),
        ];
    }
}
