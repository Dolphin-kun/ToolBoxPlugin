using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.Recording.Settings
{
    public class RecordingSettings : ToolItemSettings
    {
        private int recordingDeviceIndex = 0;
        public int RecordingDeviceIndex
        {
            get => recordingDeviceIndex;
            set => Set(ref recordingDeviceIndex, value);
        }

        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Audio;

        [JsonIgnore]
        public override string Name => "Recording";

        [JsonIgnore]
        public override string DisplayName => "録音";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("ToggleRecordingCommand");
    }
}