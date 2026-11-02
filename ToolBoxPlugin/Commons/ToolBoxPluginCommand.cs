using System.Windows.Input;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Commons
{
    public sealed class ToolBoxPluginCommand(CommandCatalogEntry entry) : PluginCommand
    {
        private const string KeyPrefix = "ToolBox.";

        public override string Name => $"{Texts.PluginName} / {entry.FullDisplayName}";

        public override ICommandId<NoParameter> Id { get; } = new PluginCommandId<NoParameter>(KeyPrefix + entry.CommandName);

        public override bool CanExecute() => Find()?.CanExecute(null) == true;

        public override void Execute()
        {
            if (Find() is { } command && command.CanExecute(null))
                command.Execute(null);
        }

        private ICommand? Find() =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault(entry.CommandName);

        public static IEnumerable<PluginCommandBase> CreateAll()
        {
            var excluded = ItemCatalog.GetDraggableItems()
                .Select(item => item.GetCommandDefinitions())
                .Where(definitions => definitions.Any(definition => definition.RequiresItemParameter))
                .SelectMany(definitions => definitions.Select(definition => definition.Name))
                .ToHashSet(StringComparer.Ordinal);

            return
            [
                .. CommandCatalog.GetEntries()
                    .Where(entry => !excluded.Contains(entry.CommandName))
                    .Select(entry => new ToolBoxPluginCommand(entry))
            ];
        }
    }
}
