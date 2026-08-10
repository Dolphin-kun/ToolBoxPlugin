using System.Windows.Controls;

namespace ToolBox.View
{
    /// <summary>
    /// 設定パネルの左ペイン。ToolBoxSettingsViewModel.Groups をツリー状に描画し、
    /// 選択されたものを SelectedDetail に反映する。
    /// </summary>
    public partial class SettingsNavigationPane : UserControl
    {
        public SettingsNavigationPane()
        {
            InitializeComponent();
        }
    }
}
