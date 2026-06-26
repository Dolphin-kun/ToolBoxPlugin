using ToolBox.Settings;
using ToolBox.Items.Shortcuts;
using YukkuriMovieMaker.Settings;
using System.Windows.Input;
using System.Text.Json.Serialization;
using ToolBox.ViewModel;

namespace ToolBox.Items.Shortcuts.Settings
{
    public class ShortcutSettings : ToolItemSettings
    {
        public override ItemCategory Category => ItemCategory.Shortcuts;
        public override string Name => Label;

        private CommandType commandType = CommandType.Delete;
        public YukkuriMovieMaker.Settings.CommandType CommandType
        {
            get => commandType;
            set => Set(ref commandType, value, etcChangedPropertyNames: nameof(IconPath));
        }

        [JsonIgnore]
        public override string IconPath => ShortcutDefinitions.Allowed
            .FirstOrDefault(d => d.CommandType == commandType)?.IconPath ?? string.Empty;

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("Shortcut");

        public ShortcutSettings()
        {
            Label = "Delete";
        }
    }
}
