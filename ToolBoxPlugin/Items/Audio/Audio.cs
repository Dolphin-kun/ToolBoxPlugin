using ToolBox.Items.Audio.Settings;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.Audio
{
    public sealed class Audio : DraggableItem
    {
        public override Type SettingsType => typeof(AudioSettings);
        public override Type ViewType => typeof(Views.AudioItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;
        public override string IconPath => "M21,3V15.5A3.5,3.5 0 0,1 17.5,19A3.5,3.5 0 0,1 14,15.5A3.5,3.5 0 0,1 17.5,12C18.04,12 18.55,12.12 19,12.34V6.47L9,8.6V17.5A3.5,3.5 0 0,1 5.5,21A3.5,3.5 0 0,1 2,17.5A3.5,3.5 0 0,1 5.5,14C6.04,14 6.55,14.12 7,14.34V6L21,3Z";

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("DetachAudioFromVideoItemCommand",
                ctx => ctx.SelectedItems?.Any(item => item is VideoItem videoItem && !string.IsNullOrEmpty(videoItem.FilePath)) == true,
                ctx => AudioCommand.DetachAudioFromVideoItem(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
