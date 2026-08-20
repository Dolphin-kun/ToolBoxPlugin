using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.SceneMover.Settings
{
    public class SceneMoverSettings : ToolItemSettings
    {
        private bool duplicateInsteadOfMove;
        public bool DuplicateInsteadOfMove
        {
            get => duplicateInsteadOfMove;
            set => Set(ref duplicateInsteadOfMove, value, etcChangedPropertyNames: nameof(InstanceSummary));
        }

        private bool alignToSceneStart;
        public bool AlignToSceneStart
        {
            get => alignToSceneStart;
            set => Set(ref alignToSceneStart, value);
        }

        private bool switchToTargetScene = true;
        public bool SwitchToTargetScene
        {
            get => switchToTargetScene;
            set => Set(ref switchToTargetScene, value);
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "SceneMover";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => "シーンへ移動";

        [JsonIgnore, NJsonIgnore]
        public override string Description =>
            "選択したアイテムを、新しいシーンや既存のシーンへまとめて移します。クリックすると移動先を選ぶメニューが開きます。";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.MovieOpenPlusOutline;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary => DuplicateInsteadOfMove ? "複製する" : "移動する";

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("MoveToNewSceneCommand");
    }
}
