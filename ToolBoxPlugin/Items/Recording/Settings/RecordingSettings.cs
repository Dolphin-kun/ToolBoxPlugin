using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

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

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Audio;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Recording";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.RecordCircleOutline;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => "録音";

        [JsonIgnore, NJsonIgnore]
        public override string Description => "マイクから録音し、録音した音声をタイムラインへ音声アイテムとして追加します。";

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("RecordingItemCommand");
    }
}
