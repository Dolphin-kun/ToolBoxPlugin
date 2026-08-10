using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Decorations.Settings;

namespace ToolBox.Items.Decorations.Views
{
    public partial class SeparatorItem : UserControl
    {
        private readonly AdornerUtil adornerUtil = new();

        public SeparatorItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not SeparatorSettings settings) return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new SeparatorSettingsPanel { DataContext = settings });
        }
    }
}
