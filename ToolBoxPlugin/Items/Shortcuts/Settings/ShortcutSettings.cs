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
        public const string TypeName = "Shortcut";

        public override ItemCategory Category => ItemCategory.Shortcuts;
        public override string Name => TypeName;

        private CommandType commandType = Commands.Delete.Type;
        [JsonConverter(typeof(CommandTypeJsonConverter))]
        public CommandType CommandType
        {
            get => commandType;
            set => Set(ref commandType, value, etcChangedPropertyNames: nameof(IconPath));
        }

        [JsonIgnore, NJsonIgnore]
        public override string HiddenKey => "Shortcut_" + CommandType;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => ShortcutDefinitions.Find(commandType)?.DefaultLabel ?? commandType.ToString();

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => ShortcutDefinitions.Find(commandType)?.IconPath ?? string.Empty;

        [JsonIgnore, NJsonIgnore]
        public override string Description => ShortcutDefinitions.Find(commandType)?.Description ?? string.Empty;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("ShortcutCommand");
    }
}
