using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.Audio.Settings
{
    public class AudioSettings : ToolItemSettings
    {
        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Audio;

        [JsonIgnore]
        public override string Name => "Audio";

        [JsonIgnore]
        public override string DisplayName => "音声分離";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("DetachAudioFromVideoItem");
    }
}
