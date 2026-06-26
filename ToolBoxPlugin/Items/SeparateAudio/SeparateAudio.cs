using ToolBox.Items.SeparateAudio.Settings;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.SeparateAudio
{
    public sealed class SeparateAudio : DraggableItem
    {
        public override Type SettingsType => typeof(SeparateAudioSettings);
        public override Type ViewType => typeof(Views.SeparateAudioItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;
        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("SeparateAudioItemCommand",
                ctx => ctx.SelectedItems?.Any(item => item is VideoItem videoItem && !string.IsNullOrEmpty(videoItem.FilePath)) == true,
                ctx => AudioCommand.DetachAudioFromVideoItem(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
