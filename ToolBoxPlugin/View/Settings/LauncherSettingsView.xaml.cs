using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.View.Settings
{
    public partial class LauncherSettingsView : UserControl
    {
        public LauncherSettingsView() => InitializeComponent();

        private LauncherSettingsViewModel? ViewModel => DataContext as LauncherSettingsViewModel;

        private void OnSelectedNodeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ViewModel is { } viewModel)
                viewModel.SelectedNode = e.NewValue as LauncherNode;
        }

        private void OnNodeDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TreeViewItem { IsSelected: true, DataContext: LauncherNode node })
                return;

            node.IsEditing = true;
            e.Handled = true;
        }

        private void OnRenameClick(object sender, RoutedEventArgs e)
        {
            if (FindNode(sender) is { } node)
                node.IsEditing = true;
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (FindNode(sender) is { } node)
                ViewModel?.RemoveNode(node);
        }

        private void OnRenameBoxVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TextBox box || e.NewValue is not true)
                return;

            box.Dispatcher.BeginInvoke(() =>
            {
                box.Focus();
                box.SelectAll();
            });
        }

        private void OnRenameBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is not (Key.Enter or Key.Escape))
                return;

            EndRename(sender);
            e.Handled = true;
        }

        private void OnRenameBoxLostFocus(object sender, RoutedEventArgs e) => EndRename(sender);

        private static void EndRename(object sender)
        {
            if (sender is FrameworkElement { DataContext: LauncherNode node })
                node.IsEditing = false;
        }

        private static LauncherNode? FindNode(object sender) =>
            (sender as FrameworkElement)?.DataContext as LauncherNode;
    }
}
