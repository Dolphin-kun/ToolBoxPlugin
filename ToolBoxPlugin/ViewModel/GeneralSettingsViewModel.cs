using ToolBox.Settings;

namespace ToolBox.ViewModel
{
    public class GeneralSettingsViewModel(ToolBoxSettings settings, ToolBoxPanel panel)
        : SettingsNodeViewModel("グリッド")
    {
        public ToolBoxSettings Settings { get; } = settings;
        public ToolBoxPanel Panel { get; } = panel;
    }
}
