using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.Launcher.Settings
{
    public class LauncherSettings : ToolItemSettings
    {
        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Utility;

        [JsonIgnore]
        public override string Name => "Launcher";

        [JsonIgnore]
        public override string DisplayName => "ファイル・フォルダーを開く";

        [JsonIgnore]
        public override string Description =>
            "指定したファイル・フォルダー・URL を開きます。素材フォルダーや外部ツールへの入口として使えます。";

        [JsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.OpenInNew;

        [JsonIgnore]
        public override string InstanceSummary =>
            string.IsNullOrWhiteSpace(TargetPath) ? "(未設定)" : TargetPath;

        [JsonIgnore]
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
