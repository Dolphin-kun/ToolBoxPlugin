using System.Windows;
using System.Windows.Controls;
using ToolBox.Items.SceneMover.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.SceneMover.Views
{
    public partial class SceneMoverSettingsPanel : UserControl
    {
        public SceneMoverSettingsPanel() => InitializeComponent();

        private void OnMoveToNewScene(object sender, RoutedEventArgs e)
        {
            if (ToolBoxViewModel.Instance is not { } context)
            {
                MessageBox.Show(
                    Texts.ToolBoxNotReady,
                    Texts.SceneMoverDisplayName,
                    MessageBoxButton.OK);
                return;
            }

            SceneMoverCommand.MoveToNewScene(context, DataContext as SceneMoverSettings);
        }
    }
}
