using ToolBox.Settings;
using YukkuriMovieMaker.Settings;
using System.Windows.Input;
using System.Text.Json.Serialization;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Shortcuts.Settings
{
    public class ShortcutSettings : ToolItemSettings
    {
        public override ItemCategory Category => ItemCategory.Shortcuts;
        public override string Name => Label;

        private CommandType commandType = CommandType.Delete;
        public CommandType CommandType
        {
            get => commandType;
            set => Set(ref commandType, value, etcChangedPropertyNames: nameof(IconPath));
        }

        [JsonIgnore, NJsonIgnore]
        public override string HiddenKey => "Shortcut_" + CommandType;

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => ShortcutDefinitions.Allowed
            .FirstOrDefault(d => d.CommandType == commandType)?.IconPath ?? string.Empty;

        [JsonIgnore, NJsonIgnore]
        public override string Description => ShortcutDefinitions.Allowed
            .FirstOrDefault(d => d.CommandType == commandType)?.Description ?? string.Empty;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("ShortcutCommand");

        public ShortcutSettings()
        {
            Label = "Delete";
        }
    }
}
