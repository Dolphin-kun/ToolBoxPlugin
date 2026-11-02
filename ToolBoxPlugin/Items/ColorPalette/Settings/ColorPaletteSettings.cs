using System.Text.Json.Serialization;
using System.Windows;
using ToolBox.Settings;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.ColorPalette.Settings
{
    public class ColorPaletteSettings : ToolItemSettings
    {
        private string paletteName = string.Empty;
        public string PaletteName { get => paletteName; set => Set(ref paletteName, value ?? string.Empty); }

        public ColorPaletteSettings()
        {
            Size = new Size(3, 1);
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Utility;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "ColorPalette";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Palette;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.ColorPaletteDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.ColorPaletteDescription;

        [JsonIgnore, NJsonIgnore]
        public override bool IsInvokable => false;
    }
}
