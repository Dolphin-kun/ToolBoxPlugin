using System.Text.Json.Serialization;
using ToolBox.Commons;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Settings;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Macro.Settings
{
    public enum MacroStepKind
    {
        Command,
        Shortcut,
        Wait,
        PanelItem,
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

        private Guid panelItemId;
        public Guid PanelItemId
        {
            get => panelItemId;
            set => Set(ref panelItemId, value, etcChangedPropertyNames: nameof(DisplayText));
        }

        [JsonIgnore, NJsonIgnore]
        public string DisplayText => Kind switch
        {
            MacroStepKind.Wait => string.Format(Texts.MacroStepWait, WaitMilliseconds),
            MacroStepKind.Shortcut => Texts.StepKindShortcut + " / " + ShortcutDisplayName(),
            MacroStepKind.PanelItem => Texts.MacroPanelItemLabel + " / " + PanelItemDisplayName(),
            _ when string.IsNullOrEmpty(CommandName) => Texts.MacroStepNew,
            _ => CommandCatalog.Find(CommandName)?.FullDisplayName ?? string.Format(Texts.MacroStepUnknown, CommandName),
        };

        private string ShortcutDisplayName() =>
            Shortcuts.ShortcutDefinitions.Allowed
                .FirstOrDefault(d => d.CommandType == ShortcutCommand)?.DisplayName
            ?? ShortcutCommand.ToString();

        private string PanelItemDisplayName() =>
            ToolBoxViewModel.FindPlacedItem(PanelItemId)?.InstanceSummary
            ?? (PanelItemId == Guid.Empty ? Texts.MacroStepChooseItem : Texts.CommonNotFound);
    }
}
