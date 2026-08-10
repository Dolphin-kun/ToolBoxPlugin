using ToolBox.Commons;
using ToolBox.Items.Launcher.Settings;
using ToolBox.Items.Launcher.Views;

namespace ToolBox.Items.Launcher
{
    public sealed class Launcher : DraggableItem
    {
        public override Type SettingsType => typeof(LauncherSettings);
        public override Type ViewType => typeof(LauncherItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(LauncherSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("LaunchItemCommand",
                (_, param) => param is LauncherSettings settings && LauncherCommand.CanLaunch(settings),
                (_, param) =>
                {
                    if (param is LauncherSettings settings)
                        LauncherCommand.Launch(settings);
                },
                displayName: "開く",
                requiresItemParameter: true),
        ];
    }
}
