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
        public override string DisplayName => "ファイル・フォルダーを開く";

        [JsonIgnore, NJsonIgnore]
        public override string Description =>
            "指定したファイル・フォルダー を開きます。素材フォルダーや外部ツールへの入口として使えます。";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.OpenInNew;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary =>
            string.IsNullOrWhiteSpace(TargetPath) ? "(未設定)" : TargetPath;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("LaunchItemCommand");

        private string targetPath = string.Empty;
        public string TargetPath
        {
            get => targetPath;
            set => Set(ref targetPath, value, etcChangedPropertyNames: nameof(InstanceSummary));
        }

        public LauncherSettings()
        {
            Label = "開く";
        }
    }
}
