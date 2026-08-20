using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Shortcuts;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class LauncherSettingsViewModel : SettingsNodeViewModel
    {
        private readonly ToolBoxSettings settings;
        private readonly ToolBoxPanel panel;

        public ToolBoxSettings Settings => settings;

        public ObservableCollection<LauncherNode> Nodes => settings.LauncherNodes;

        public IReadOnlyList<ToolItemSettings> AvailableItems { get; private set; } = [];

        public IReadOnlyList<CommandCatalogEntry> AvailableCommands { get; } = CommandCatalog.GetEntries();

        public IReadOnlyList<ShortcutDefinition> AvailableShortcuts { get; } = ShortcutDefinitions.Allowed;

        private LauncherNodeKind addKind = LauncherNodeKind.PanelItem;

        public LauncherNodeKind AddKind
        {
            get => addKind;
            set
            {
                if (Set(ref addKind, value))
                    (AddCommand as ActionCommand)?.RaiseCanExecuteChanged();
            }
        }

        private ToolItemSettings? selectedAvailableItem;
        public ToolItemSettings? SelectedAvailableItem
        {
            get => selectedAvailableItem;
            set
            {
                if (Set(ref selectedAvailableItem, value))
                    (AddCommand as ActionCommand)?.RaiseCanExecuteChanged();
            }
        }

        private CommandCatalogEntry? selectedAvailableCommand;
        public CommandCatalogEntry? SelectedAvailableCommand
        {
            get => selectedAvailableCommand;
            set
            {
                if (Set(ref selectedAvailableCommand, value))
                    (AddCommand as ActionCommand)?.RaiseCanExecuteChanged();
            }
        }

        private ShortcutDefinition? selectedAvailableShortcut;
        public ShortcutDefinition? SelectedAvailableShortcut
        {
            get => selectedAvailableShortcut;
            set
            {
                if (Set(ref selectedAvailableShortcut, value))
                    (AddCommand as ActionCommand)?.RaiseCanExecuteChanged();
            }
        }

        private LauncherNode? selectedNode;
        public LauncherNode? SelectedNode
        {
            get => selectedNode;
            set
            {
                if (!Set(ref selectedNode, value))
                    return;

                (RemoveCommand as ActionCommand)?.RaiseCanExecuteChanged();
                (MoveUpCommand as ActionCommand)?.RaiseCanExecuteChanged();
                (MoveDownCommand as ActionCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand ClearCommand { get; }

        public LauncherSettingsViewModel(ToolBoxSettings settings, ToolBoxPanel panel) : base("ランチャー")
        {
            this.settings = settings;
            this.panel = panel;

            RefreshAvailableItems();

            panel.Items.CollectionChanged += (_, _) => RefreshAvailableItems();

            SelectedAvailableCommand = AvailableCommands.FirstOrDefault();
            SelectedAvailableShortcut = AvailableShortcuts.FirstOrDefault();

            AddCommand = new ActionCommand(_ => CanAdd(), _ => Add());
            RemoveCommand = new ActionCommand(_ => SelectedNode != null, _ => Remove());
            MoveUpCommand = new ActionCommand(_ => SelectedNode != null, _ => Move(-1));
            MoveDownCommand = new ActionCommand(_ => SelectedNode != null, _ => Move(1));
            ClearCommand = new ActionCommand(_ => Nodes.Count > 0, _ => Clear());

            Nodes.CollectionChanged += (_, _) =>
                (ClearCommand as ActionCommand)?.RaiseCanExecuteChanged();
        }

        public void RefreshAvailableItems()
        {
            AvailableItems =
            [
                .. panel.Items
                    .Where(item => item.IsInvokable && item.DefaultCommand != null)
                    .OrderBy(item => item.Position.Y)
                    .ThenBy(item => item.Position.X)
            ];

            SelectedAvailableItem ??= AvailableItems.FirstOrDefault();
            OnPropertyChanged(nameof(AvailableItems));
            (AddCommand as ActionCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanAdd() => AddKind switch
        {
            LauncherNodeKind.PanelItem => SelectedAvailableItem != null,
            LauncherNodeKind.Command => SelectedAvailableCommand != null,
            LauncherNodeKind.Shortcut => SelectedAvailableShortcut != null,
            _ => true,
        };

        private void Add()
        {
            var node = AddKind switch
            {
                LauncherNodeKind.PanelItem when SelectedAvailableItem is { } item =>
                    LauncherNode.CreateItem(item),
                LauncherNodeKind.Command when SelectedAvailableCommand is { } command =>
                    LauncherNode.CreateCommand(command),
                LauncherNodeKind.Shortcut when SelectedAvailableShortcut is { } shortcut =>
                    LauncherNode.CreateShortcut(shortcut),
                LauncherNodeKind.Folder => LauncherNode.CreateFolder("新しいグループ"),
                _ => null,
            };

            if (node != null)
                TargetCollection().Add(node);
        }

        private ObservableCollection<LauncherNode> TargetCollection() =>
            SelectedNode is { IsFolder: true } folder ? folder.Children : Nodes;

        private void Remove()
        {
            if (SelectedNode is { } node)
                RemoveNode(node);
        }

        private void Clear()
        {
            var confirm = MessageBox.Show(
                "ランチャーの並びをすべて消します。\n" +
                "消したあとは、今のパネルのアイテムが自動で並ぶ状態に戻ります。",
                "ランチャー",
                MessageBoxButton.OKCancel);

            if (confirm != MessageBoxResult.OK)
                return;

            Nodes.Clear();
            SelectedNode = null;
        }

        public void RemoveNode(LauncherNode node)
        {
            FindParent(Nodes, node)?.Remove(node);

            if (SelectedNode == node)
                SelectedNode = null;
        }

        private void Move(int offset)
        {
            if (SelectedNode is not { } node)
                return;

            var parent = FindParent(Nodes, node);
            if (parent == null)
                return;

            var index = parent.IndexOf(node);
            var newIndex = index + offset;

            if (newIndex < 0 || newIndex >= parent.Count)
                return;

            parent.Move(index, newIndex);
        }

        private static ObservableCollection<LauncherNode>? FindParent(
            ObservableCollection<LauncherNode> collection, LauncherNode target)
        {
            if (collection.Contains(target))
                return collection;

            foreach (var node in collection)
            {
                if (FindParent(node.Children, target) is { } found)
                    return found;
            }

            return null;
        }
    }
}
