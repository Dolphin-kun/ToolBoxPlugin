using YukkuriMovieMaker.Commons;
using ToolBox.Items.Recording.Settings;

namespace ToolBox.Items.Recording.ViewModels
{
    public class RecordingSettingsViewModel(RecordingSettings settings) : Bindable
    {
        public RecordingSettings Settings { get; } = settings;

        public RecordingSettingsViewModel() : this(new RecordingSettings())
        {
        }
    }
}
