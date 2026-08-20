using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using ToolBox.Commons;
using ToolBox.Items.Shortcuts;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Settings;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Settings
{
    public enum LauncherNodeKind
    {
        Folder,

        PanelItem,

        Command,

        Shortcut,
    }

    public class LauncherNode : Bindable
    {
        private static readonly string[] LabelPropertyNames = [nameof(DisplayLabel)];

        private string title = string.Empty;

        public string Title
        {
            get => title;
            set => Set(ref title, value, etcChangedPropertyNames: LabelPropertyNames);
        }

        private LauncherNodeKind kind = LauncherNodeKind.Folder;
        public LauncherNodeKind Kind
        {
            get => kind;
            set => Set(ref kind, value, etcChangedPropertyNames: [nameof(IsFolder), .. LabelPropertyNames]);
        }

        private Guid itemId;
        public Guid ItemId
        {
            get => itemId;
            set => Set(ref itemId, value, etcChangedPropertyNames: [nameof(IsFolder), .. LabelPropertyNames]);
        }

        private string commandName = string.Empty;
        public string CommandName
        {
            get => commandName;
            set => Set(ref commandName, value, etcChangedPropertyNames: LabelPropertyNames);
        }

        private CommandType shortcutCommand = CommandType.Undo;
        public CommandType ShortcutCommand
        {
            get => shortcutCommand;
            set => Set(ref shortcutCommand, value, etcChangedPropertyNames: LabelPropertyNames);
        }

        private bool isEditing;

        [JsonIgnore, NJsonIgnore]
        public bool IsEditing
        {
            get => isEditing;
            set => Set(ref isEditing, value);
        }

        private ObservableCollection<LauncherNode> children = [];
        public ObservableCollection<LauncherNode> Children
        {
            get => children;
            set => Set(ref children, value ?? []);
        }

        [JsonIgnore, NJsonIgnore]
        public LauncherNodeKind EffectiveKind =>
            Kind == LauncherNodeKind.Folder && ItemId != Guid.Empty
                ? LauncherNodeKind.PanelItem
                : Kind;

        [JsonIgnore, NJsonIgnore]
        public bool IsFolder => EffectiveKind == LauncherNodeKind.Folder;

        [JsonIgnore, NJsonIgnore]
        public string DisplayLabel
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Title))
                    return Title;

                return EffectiveKind switch
                {
                    LauncherNodeKind.PanelItem =>
                        ToolBoxViewModel.FindPlacedItem(ItemId)?.Label ?? "(見つかりません)",
                    LauncherNodeKind.Command =>
                        CommandCatalog.Find(CommandName)?.DisplayName ?? "(見つかりません)",
                    LauncherNodeKind.Shortcut =>
                        FindShortcut()?.DisplayName ?? ShortcutCommand.ToString(),
                    _ => "グループ",
                };
            }
        }

        public ShortcutDefinition? FindShortcut() =>
            ShortcutDefinitions.Allowed.FirstOrDefault(d => d.CommandType == ShortcutCommand);

        public static LauncherNode CreateFolder(string title) =>
            new() { Kind = LauncherNodeKind.Folder, Title = title };

        public static LauncherNode CreateItem(ToolItemSettings item) =>
            new() { Kind = LauncherNodeKind.PanelItem, ItemId = item.Id };

        public static LauncherNode CreateCommand(CommandCatalogEntry entry) =>
            new() { Kind = LauncherNodeKind.Command, CommandName = entry.CommandName };

        public static LauncherNode CreateShortcut(ShortcutDefinition definition) =>
            new() { Kind = LauncherNodeKind.Shortcut, ShortcutCommand = definition.CommandType };
    }
}
