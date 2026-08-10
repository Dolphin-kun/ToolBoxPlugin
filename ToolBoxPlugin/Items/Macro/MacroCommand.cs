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

        public static async Task RunAsync(IToolBoxContext ctx, MacroSettings settings)
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

                        default:
                            if (!RunCommand(ctx, step.CommandName, settings))
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

        private static bool RunCommand(IToolBoxContext ctx, string commandName, MacroSettings settings)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                return true;

            var command = ToolBoxViewModel.Instance?.Commands.GetValueOrDefault(commandName);
            if (command == null)
            {
                MessageBox.Show(
                    $"マクロ「{settings.Label}」の手順「{commandName}」が見つかりませんでした。\n" +
                    "提供元のアドオンが外されている可能性があります。",
                    "マクロ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (command.CanExecute(null))
                command.Execute(null);

            return true;
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
