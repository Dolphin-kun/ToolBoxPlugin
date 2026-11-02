using ToolBox.Commons;
using ToolBox.Settings;

namespace ToolBox.ViewModel
{
    public class BasicSettingsViewModel : SettingsNodeViewModel
    {
        public ToolBoxSettings Settings { get; }

        public UpdateChecker Update { get; } = UpdateChecker.Instance;

        public BasicSettingsViewModel(ToolBoxSettings settings) : base(Texts.BasicSettingsTitle)
        {
            Settings = settings;
            Update.EnsureChecked();
        }
    }
}
