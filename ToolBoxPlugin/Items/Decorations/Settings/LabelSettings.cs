using System.Text.Json.Serialization;
using ToolBox.Settings;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Decorations.Settings
{
    public class LabelSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Decoration;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Label";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => "ラベル";

        [JsonIgnore, NJsonIgnore]
        public override string Description => "パネルに見出しの文字を置きます。クリックしても何も実行しません。";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.FormatText;

        [JsonIgnore, NJsonIgnore]
        public override bool IsInvokable => false;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary => string.IsNullOrWhiteSpace(Text) ? DisplayName : Text;

        private string text = "見出し";
        public string Text
        {
            get => text;
            set => Set(ref text, value, etcChangedPropertyNames: nameof(InstanceSummary));
        }

        private double fontSize = 12;
        public double FontSize
        {
            get => fontSize;
            set => Set(ref fontSize, value);
        }

        private bool isBold = true;
        public bool IsBold
        {
            get => isBold;
            set => Set(ref isBold, value);
        }

        private ItemTextAlignment textAlignment = ItemTextAlignment.Left;
        public ItemTextAlignment TextAlignment
        {
            get => textAlignment;
            set => Set(ref textAlignment, value);
        }

        public LabelSettings()
        {
            DisplayMode = ToolItemDisplayMode.Icon;
        }
    }
}
