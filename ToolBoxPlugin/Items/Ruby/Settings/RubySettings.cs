using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.Ruby.Settings
{
    public class RubySettings : ToolItemSettings
    {
        private RubyType rubyType = RubyType.Hiragana;
        public RubyType RubyType { get => rubyType; set => Set(ref rubyType, value); }

        private bool isAutoAdjustLineHeight = true;
        public bool IsAutoAdjustLineHeight { get => isAutoAdjustLineHeight; set => Set(ref isAutoAdjustLineHeight, value); }

        private double lineHeightMultiplier = 1.5;
        public double LineHeightMultiplier { get => lineHeightMultiplier; set => Set(ref lineHeightMultiplier, value); }

        [JsonIgnore]
        public override System.Windows.Size MinSize => new(1, 1);

        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Text;

        [JsonIgnore]
        public override string Name => "Ruby";

        [JsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.FuriganaHorizontal;

        [JsonIgnore]
        public override string DisplayName => "ルビ振り";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("RubyItemCommand");
    }
}
