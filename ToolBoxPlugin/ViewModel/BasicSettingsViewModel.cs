using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class BasicSettingsViewModel(ToolBoxSettings settings) : Bindable
    {
        public ToolBoxSettings Settings { get; } = settings;
        public string CategoryName { get; } = "基本設定";

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }
    }
}
