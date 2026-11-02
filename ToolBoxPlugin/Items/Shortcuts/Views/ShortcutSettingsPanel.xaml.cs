using System.Windows.Controls;

namespace ToolBox.Items.Shortcuts.Views
{
    public partial class ShortcutSettingsPanel : UserControl
    {
        public ShortcutSettingsPanel()
        {
            InitializeComponent();

            CommandTypeComboBox.ItemsSource = ShortcutDefinitions.Allowed;
        }
    }
}
