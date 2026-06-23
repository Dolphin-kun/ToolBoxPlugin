using System.Windows;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class GeneralSettingsViewModel(ToolBoxSettings settings) : Bindable
    {
        public ToolBoxSettings Settings { get; } = settings;
        public string CategoryName { get; } = "グリッド";

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }
    }
}
