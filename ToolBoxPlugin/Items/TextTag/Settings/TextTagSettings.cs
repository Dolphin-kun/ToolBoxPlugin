using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.TextTag.Settings
{
    public class TextTagSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(1, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Text;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "TextTag";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.TagText;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.TextTagDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.TextTagDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("TextTagCommand");
    }
}
