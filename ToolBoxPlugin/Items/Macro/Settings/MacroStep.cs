using System.Text.Json.Serialization;
using ToolBox.Commons;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Items.Macro.Settings
{
    public enum MacroStepKind
    {
        Command,
        Shortcut,
        Wait,
    }

    public class MacroStep : Bindable
    {
        private MacroStepKind kind = MacroStepKind.Command;
        public MacroStepKind Kind
        {
            get => kind;
            set => Set(ref kind, value, etcChangedPropertyNames: nameof(DisplayText));
        }

        private string commandName = string.Empty;
        public string CommandName
        {
            get => commandName;
            set => Set(ref commandName, value, etcChangedPropertyNames: nameof(DisplayText));
        }

        private CommandType shortcutCommand = CommandType.Undo;
        public CommandType ShortcutCommand
        {
            get => shortcutCommand;
            set => Set(ref shortcutCommand, value, etcChangedPropertyNames: nameof(DisplayText));
        }

        private int waitMilliseconds = 200;
        public int WaitMilliseconds
        {
            get => waitMilliseconds;
            set => Set(ref waitMilliseconds, value, etcChangedPropertyNames: nameof(DisplayText));
        }

        [JsonIgnore]
        public string DisplayText => Kind switch
        {
            MacroStepKind.Wait => $"{WaitMilliseconds} ミリ秒待つ",
            MacroStepKind.Shortcut => "ショートカット / " + ShortcutDisplayName(),
            _ when string.IsNullOrEmpty(CommandName) => "新しい手順 (操作を選んでください)",
            _ => CommandCatalog.Find(CommandName)?.FullDisplayName ?? $"(不明な操作: {CommandName})",
        };

        private string ShortcutDisplayName() =>
            Shortcuts.ShortcutDefinitions.Allowed
                .FirstOrDefault(d => d.CommandType == ShortcutCommand)?.DisplayName
            ?? ShortcutCommand.ToString();
    }
}
