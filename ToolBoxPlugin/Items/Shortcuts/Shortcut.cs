using System.Windows;
using System.Windows.Input;
using ToolBox.Items.Shortcuts.Settings;
using ToolBox.Items.Shortcuts.Views;
using YukkuriMovieMaker.Settings;

namespace ToolBox.Items.Shortcuts
{
    public sealed class Shortcut : DraggableItem
    {
        public override Type SettingsType => typeof(ShortcutSettings);
        public override Type ViewType => typeof(ShortcutItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(ShortcutSettingsPanel);
        public override Type? SettingsViewModelType => null;
        public override string IconPath => string.Empty;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("ShortcutCommand",
                (ctx, param) => true,
                (ctx, param) => 
                {
                    if (param is not ShortcutSettings settings) return;
                    
                    var cmdObj = CommandSettings.Default[settings.CommandType];
                    
                    if (cmdObj is RoutedCommand routedCmd)
                    {
                        var mainWindow = Application.Current.MainWindow;
                        if (routedCmd.CanExecute(null, mainWindow))
                        {
                            routedCmd.Execute(null, mainWindow);
                        }
                    }
                })
        ];
    }
}
