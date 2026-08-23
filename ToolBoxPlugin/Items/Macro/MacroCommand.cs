using System.Windows;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Macro.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Items.Macro
{
    public static class MacroCommand
    {
        private static bool isRunning;

        public static bool CanRun(MacroSettings settings) => !isRunning && settings.Steps.Count > 0;

        public static void Run(MacroSettings settings) => _ = RunAsync(settings);

        private static async Task RunAsync(MacroSettings settings)
        {
            if (isRunning)
                return;

            isRunning = true;
            try
            {
                foreach (var step in settings.Steps.ToList())
                {
                    switch (step.Kind)
                    {
                        case MacroStepKind.Wait:
                            await Task.Delay(Math.Max(0, step.WaitMilliseconds));
                            break;

                        case MacroStepKind.Shortcut:
                            RunShortcut(step.ShortcutCommand);
                            break;

                        case MacroStepKind.PanelItem:
                            RunPanelItem(step.PanelItemId);
                            break;

                        default:
                            if (!RunCommand(step.CommandName, settings))
                                return;
                            break;
                    }
                }
            }
            finally
            {
                isRunning = false;
            }
        }

        private static bool RunCommand(string commandName, MacroSettings settings)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                return true;

            var command = ToolBoxViewModel.Instance?.Commands.GetValueOrDefault(commandName);
            if (command == null)
            {
                MessageBox.Show(
                    $"{string.Format(Texts.MacroStepNotFound, settings.Label, commandName)}\n" +
                    Texts.MacroStepNotFoundHint,
                    Texts.MacroDisplayName,
                    MessageBoxButton.OK);
                return false;
            }

            if (command.CanExecute(null))
                command.Execute(null);

            return true;
        }

        private static void RunPanelItem(Guid itemId)
        {
            var item = ToolBoxViewModel.FindPlacedItem(itemId);
            if (item == null)
                return;

            var command = item.DefaultCommand;
            if (command?.CanExecute(item) == true)
                command.Execute(item);
        }

        private static void RunShortcut(CommandType commandType)
        {
            if (CommandSettings.Default[commandType] is not RoutedCommand routed)
                return;

            var mainWindow = Application.Current?.MainWindow;
            if (routed.CanExecute(null, mainWindow))
                routed.Execute(null, mainWindow);
        }
    }
}
