using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.Sort.Settings
{
    public class SortSettings : ToolItemSettings
    {
        [JsonIgnore]
        public override Size MinSize => new(2, 1);

        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore]
        public override string Name => "Sort";

        [JsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Sort;

        [JsonIgnore]
        public override string DisplayName => "整列";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("SortAscendingItemCommand");
    }
}
