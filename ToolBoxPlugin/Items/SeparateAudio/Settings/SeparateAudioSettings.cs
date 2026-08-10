using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.SeparateAudio.Settings
{
    public class SeparateAudioSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Audio;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "SeparateAudio";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => "音声分離";

        [JsonIgnore, NJsonIgnore]
        public override string Description => "選択した動画アイテムの音量を 0 にして、同じ音声を別の音声アイテムとして 1 つ下のレイヤーに追加します。";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => "M15 5.5H18V7.5H16V12.5C16 13.605 15.105 14.5 14 14.5S12 13.605 12 12.5 12.895 10.5 14 10.5C14.365 10.5 14.705 10.605 15 10.775V5.5ZM9.5 14.5H10.5L11.5 5.5H10.5L9.5 14.5ZM7.4 9.5 8.7 8.2V12L7.4 10.6V11.8A.3.3 90 017.1 12.1H3.3A.3.3 90 013 11.8V8.3A.3.3 90 013.3 8H7.1A.3.3 90 017.4 8.3V9.5Z";

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("SeparateAudioItemCommand");
    }
}
