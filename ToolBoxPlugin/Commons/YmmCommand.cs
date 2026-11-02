using System.Windows;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Commons
{
    public static class YmmCommand
    {
        private static IInputElement? Target => Application.Current?.MainWindow;

        public static bool CanExecute(ICommandId<NoParameter> id) =>
            Guard(id, () => CommandRegistry.CanExecute(id, Target));

        public static bool Execute(ICommandId<NoParameter> id) =>
            Guard(id, () =>
            {
                var target = Target;
                if (!CommandRegistry.CanExecute(id, target))
                    return false;

                CommandRegistry.Execute(id, target);
                return true;
            });

        public static bool CanExecute<TParameter>(ICommandId<TParameter> id, TParameter parameter) =>
            Guard(id, () => CommandRegistry.CanExecute(id, parameter, Target));

        public static bool Execute<TParameter>(ICommandId<TParameter> id, TParameter parameter) =>
            Guard(id, () =>
            {
                var target = Target;
                if (!CommandRegistry.CanExecute(id, parameter, target))
                    return false;

                CommandRegistry.Execute(id, parameter, target);
                return true;
            });

        private static bool Guard(ICommandId id, Func<bool> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to run {id.Key}: {ex}");
                return false;
            }
        }
    }
}
