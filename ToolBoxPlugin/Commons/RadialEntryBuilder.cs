using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;
using ToolBox.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Commons
{
    public static class RadialEntryBuilder
    {
        public const int MaxItems = 8;

        public static IReadOnlyList<RadialEntry> BuildRoot()
        {
            var nodes = ToolBoxSettings.Default.LauncherNodes;

            if (nodes.Count == 0)
                return BuildFromPanel();

            var entries = Build(nodes);
            return entries.Count > 0 ? entries : BuildFromPanel();
        }

        public static IReadOnlyList<RadialEntry> Build(IEnumerable<LauncherNode> nodes)
        {
            var entries = new List<RadialEntry>();

            foreach (var node in nodes.Take(MaxItems))
            {
                if (CreateEntry(node) is { } entry)
                    entries.Add(entry);
            }

            return entries;
        }

        private static RadialEntry? CreateEntry(LauncherNode node) => node.EffectiveKind switch
        {
            LauncherNodeKind.Folder => new RadialEntry
            {
                Label = node.DisplayLabel,
                Children = Build(node.Children),
            },
            LauncherNodeKind.PanelItem => CreatePanelItemEntry(node),
            LauncherNodeKind.Command => CreateCommandEntry(node),
            LauncherNodeKind.Shortcut => CreateShortcutEntry(node),
            _ => null,
        };

        private static RadialEntry? CreatePanelItemEntry(LauncherNode node)
        {
            var item = ToolBoxViewModel.FindPlacedItem(node.ItemId);
            return item == null ? null : CreateItemEntry(item, node.Title);
        }

        private static RadialEntry? CreateCommandEntry(LauncherNode node)
        {
            var definition = CommandCatalog.Find(node.CommandName);
            if (definition == null)
                return null;

            return new RadialEntry
            {
                Label = string.IsNullOrWhiteSpace(node.Title) ? definition.DisplayName : node.Title,
                IconPath = definition.IconPath,
                CanInvoke = () => FindCommand(node.CommandName)?.CanExecute(null) == true,
                Invoke = () => FindCommand(node.CommandName)?.Execute(null),
            };
        }

        private static RadialEntry CreateShortcutEntry(LauncherNode node)
        {
            var definition = node.FindShortcut();
            var commandType = node.ShortcutCommand;

            return new RadialEntry
            {
                Label = string.IsNullOrWhiteSpace(node.Title)
                    ? definition?.DisplayName ?? commandType.ToString()
                    : node.Title,
                IconPath = definition?.IconPath ?? string.Empty,
                CanInvoke = () => CanRunShortcut(commandType),
                Invoke = () => RunShortcut(commandType),
            };
        }

        private static ICommand? FindCommand(string commandName) =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault(commandName);

        private static bool CanRunShortcut(CommandType commandType) =>
            CommandSettings.Default[commandType] is RoutedCommand routed
            && routed.CanExecute(null, Application.Current?.MainWindow);

        private static void RunShortcut(CommandType commandType)
        {
            if (CommandSettings.Default[commandType] is not RoutedCommand routed)
                return;

            var mainWindow = Application.Current?.MainWindow;
            if (routed.CanExecute(null, mainWindow))
                routed.Execute(null, mainWindow);
        }

        public static IReadOnlyList<RadialEntry> BuildFromPanel()
        {
            var viewModel = ToolBoxViewModel.Instance;
            if (viewModel == null)
                return [];

            return
            [
                .. viewModel.Panel.Items
                    .Where(item => item.IsInvokable && item.DefaultCommand != null)
                    .OrderBy(item => item.Position.Y)
                    .ThenBy(item => item.Position.X)
                    .Take(MaxItems)
                    .Select(item => CreateItemEntry(item, string.Empty))
            ];
        }

        private static RadialEntry CreateItemEntry(ToolItemSettings item, string title) => new()
        {
            Label = string.IsNullOrWhiteSpace(title) ? item.Label : title,
            IconPath = item.IconPath,
            Foreground = item.AppearanceForeground,
            CanInvoke = () => item.DefaultCommand?.CanExecute(item) == true,
            Invoke = () =>
            {
                if (item.DefaultCommand is { } command && command.CanExecute(item))
                    command.Execute(item);
            },
        };
    }
}
