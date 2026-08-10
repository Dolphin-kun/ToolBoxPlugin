using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class GeneralSettingsViewModel(ToolBoxSettings settings, ToolBoxPanel panel) : Bindable
    {
        public ToolBoxSettings Settings { get; } = settings;

        public ToolBoxPanel Panel { get; } = panel;

        public string CategoryName { get; } = "グリッド";

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }
    }
}
