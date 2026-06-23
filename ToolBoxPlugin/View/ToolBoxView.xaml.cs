using System.Windows;
using System.Windows.Controls;
using ToolBox.ViewModel;

namespace ToolBox.View
{
    public partial class ToolBoxView : UserControl
    {

        public ToolBoxView()
        {
            InitializeComponent();
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ToolBoxViewModel vm) return;

            e.Handled = true;
            vm.IsSettingsVisible = true;
        }
    }
}
