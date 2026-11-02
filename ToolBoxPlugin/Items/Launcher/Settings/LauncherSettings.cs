using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Launcher.Settings
{
    public class LauncherSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Utility;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Launcher";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.LauncherDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description =>
            Texts.LauncherDescription;

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.OpenInNew;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary =>
            string.IsNullOrWhiteSpace(TargetPath) ? Texts.CommonUnset : TargetPath;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("LaunchItemCommand");

        private string targetPath = string.Empty;
        public string TargetPath
        {
            get => targetPath;
            set => Set(ref targetPath, value, etcChangedPropertyNames: nameof(InstanceSummary));
        }

    }
}
