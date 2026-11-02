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

        public ObservableCollection<LauncherNodeRow> Rows { get; } = [];

        private readonly List<ObservableCollection<LauncherNode>> watchedCollections = [];

        private LauncherNodeRow? selectedRow;
        public LauncherNodeRow? SelectedRow
        {
            get => selectedRow;
            set
            {
                if (Set(ref selectedRow, value))
                    SelectedNode = value?.Node;
            }
        }

        private IReadOnlyList<ToolItemSettings> availableItems = [];

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

        public ICommand RemoveCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand ClearCommand { get; }

        public LauncherSettingsViewModel(ToolBoxSettings settings, ToolBoxPanel panel) : base(Texts.LauncherSettingsNodeTitle)
        {
            this.settings = settings;
            this.panel = panel;

            RefreshAvailableItems();

            panel.Items.CollectionChanged += (_, _) => RefreshAvailableItems();

            RemoveCommand = new ActionCommand(_ => SelectedNode != null, _ => Remove());
            MoveUpCommand = new ActionCommand(_ => SelectedNode != null, _ => Move(-1));
            MoveDownCommand = new ActionCommand(_ => SelectedNode != null, _ => Move(1));
            ClearCommand = new ActionCommand(_ => Nodes.Count > 0, _ => Clear());

            Nodes.CollectionChanged += (_, _) =>
                (ClearCommand as ActionCommand)?.RaiseCanExecuteChanged();

            RebuildRows();
        }

        private void RebuildRows()
        {
            foreach (var collection in watchedCollections)
                collection.CollectionChanged -= OnNodesChanged;
            watchedCollections.Clear();

            var selected = SelectedNode;
            Rows.Clear();
            AddRows(Nodes, 0);
            SelectedRow = Rows.FirstOrDefault(row => row.Node == selected);
        }

        private void AddRows(ObservableCollection<LauncherNode> nodes, int depth)
        {
            nodes.CollectionChanged += OnNodesChanged;
            watchedCollections.Add(nodes);

            foreach (var node in nodes)
            {
                Rows.Add(new LauncherNodeRow(node, depth));
                AddRows(node.Children, depth + 1);
            }
        }

        private void OnNodesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => RebuildRows();

        public void RefreshAvailableItems()
        {
            availableItems =
            [
                .. panel.Items
                    .Where(item => item.IsInvokable && item.DefaultCommand != null)
                    .OrderBy(item => item.Position.Y)
                    .ThenBy(item => item.Position.X)
            ];
        }

        public IReadOnlyList<LauncherCandidate> CreateCandidates()
        {
            var panelGroup = Texts.LauncherKindPanelItem;

            return
            [
                .. availableItems.Select(item => new LauncherCandidate(
                    item.Label, panelGroup, item.IconPath, () => LauncherNode.CreateItem(item))),
                .. CommandCatalog.GetEntries().Select(entry => new LauncherCandidate(
                    entry.DisplayName, $"{Texts.StepKindCommand} / {entry.GroupName}", entry.IconPath, () => LauncherNode.CreateCommand(entry))),
                .. ShortcutDefinitions.Allowed.Select(definition => new LauncherCandidate(
                    definition.DisplayName, $"{Texts.StepKindShortcut} / {definition.Group}", definition.IconPath, () => LauncherNode.CreateShortcut(definition))),
            ];
        }

        public void AddNodes(IEnumerable<LauncherNode> nodes)
        {
            var target = TargetCollection();
            foreach (var node in nodes)
                target.Add(node);
        }

        public void AddFolder() => TargetCollection().Add(LauncherNode.CreateFolder(Texts.LauncherNewGroup));

        private ObservableCollection<LauncherNode> TargetCollection() =>
            SelectedNode is { IsFolder: true } folder ? folder.Children : Nodes;

        private void Remove()
        {
            if (SelectedNode is { } node)
                RemoveNode(node);
        }

        public void Clear()
        {
            var confirm = MessageBox.Show(
                $"{Texts.LauncherClearConfirm}\n" +
                Texts.LauncherClearConfirmHint,
                Texts.LauncherSettingsNodeTitle,
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
