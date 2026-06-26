using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.Staircase.Settings
{
    public class StaircaseSettings : ToolItemSettings
    {
        private int offset = 20;
        public int Offset { get => offset; set => Set(ref offset, Math.Max(1, value)); }

        [JsonIgnore]
        public override Size MinSize => new(2, 1);

        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore]
        public override string Name => "Staircase";

        [JsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.SortReverseVariant;

        [JsonIgnore]
        public override string DisplayName => "階段状に配置";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("StaircaseItemCommand");
    }
}
