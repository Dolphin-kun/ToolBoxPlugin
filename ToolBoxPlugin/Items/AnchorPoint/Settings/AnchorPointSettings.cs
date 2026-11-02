using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.AnchorPoint.Settings
{
    public class AnchorPointSettings : ToolItemSettings
    {
        public AnchorPointSettings()
        {
            Size = new Size(2, 2);
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Animation;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "AnchorPoint";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.ImageFilterCenterFocus;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.AnchorPointDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.AnchorPointDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("AnchorPointCenterCommand");
    }
}
