using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Ruby.Settings
{
    public enum RubyType
    {
        [Display(Name = nameof(Texts.RubyTypeHiragana), Description = nameof(Texts.RubyTypeHiraganaDescription), ResourceType = typeof(Texts))]
        Hiragana,

        [Display(Name = nameof(Texts.RubyTypeKatakana), Description = nameof(Texts.RubyTypeKatakanaDescription), ResourceType = typeof(Texts))]
        Katakana,
    }

    public class RubySettings : ToolItemSettings
    {
        private RubyType rubyType = RubyType.Hiragana;
        public RubyType RubyType { get => rubyType; set => Set(ref rubyType, value); }

        private bool isAutoAdjustLineHeight = true;
        public bool IsAutoAdjustLineHeight { get => isAutoAdjustLineHeight; set => Set(ref isAutoAdjustLineHeight, value); }

        private double lineHeightMultiplier = 1.5;
        public double LineHeightMultiplier { get => lineHeightMultiplier; set => Set(ref lineHeightMultiplier, value); }

        [JsonIgnore, NJsonIgnore]
        public override System.Windows.Size MinSize => new(1, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Text;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Ruby";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.FuriganaHorizontal;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.RubyDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.RubyDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("RubyItemCommand");
    }
}
