using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class GeneralSettingsViewModel(ToolBoxSettings settings, ToolBoxPanel panel) : Bindable
    {
        /// <summary>全パネル共通の見た目の設定。</summary>
        public ToolBoxSettings Settings { get; } = settings;

        /// <summary>このパネル固有の設定 (グリッド数)。</summary>
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
