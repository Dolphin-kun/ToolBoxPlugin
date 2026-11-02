using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ToolBox.Controls;
using ToolBox.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Resources.Icons;

namespace ToolBox.View.Settings
{
    public partial class LauncherSettingsView : UserControl
    {
        public LauncherSettingsView()
        {
            InitializeComponent();

            var list = NodeList.ListBox!;
            list.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(LauncherSettingsViewModel.Rows)));
            list.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedItemProperty,
                new Binding(nameof(LauncherSettingsViewModel.SelectedRow)) { Mode = BindingMode.TwoWay });
            list.ItemTemplate = (DataTemplate)FindResource("NodeRowTemplate");
            NodeList.ItemContainerSetters.Add(new EventSetter(MouseDoubleClickEvent, new MouseButtonEventHandler(OnNodeDoubleClick)));

            DataContextChanged += (_, _) => BuildButtons();
        }

        private LauncherSettingsViewModel? ViewModel => DataContext as LauncherSettingsViewModel;

        private void BuildButtons()
        {
            if (ViewModel is not { } viewModel || NodeList.Tag == viewModel)
                return;

            NodeList.Tag = viewModel;
            NodeList.ClearButtons();
            NodeList.AddMenuButton(GeometryKeys.Plus, Texts.CommonAdd, menu =>
            {
                menu.Items.Add(ListPanel.MenuItem(Texts.LauncherAddEntriesMenu, () => AddEntries(viewModel)));
                menu.Items.Add(ListPanel.MenuItem(Texts.LauncherAddGroupMenu, viewModel.AddFolder));
            });
            NodeList.AddCommandButton(GeometryKeys.Minus, Texts.CommonDelete, viewModel.RemoveCommand);
            NodeList.AddCommandButton(GeometryKeys.MenuUp, Texts.CommonMoveUp, viewModel.MoveUpCommand);
            NodeList.AddCommandButton(GeometryKeys.MenuDown, Texts.CommonMoveDown, viewModel.MoveDownCommand);
            NodeList.AddMenuButton(GeometryKeys.DotsVertical, Texts.CommonMoreActions, menu =>
            {
                menu.Items.Add(ListPanel.MenuItem(Texts.CommonRename, () => viewModel.SelectedNode?.IsEditing = true, viewModel.SelectedNode != null));
                menu.Items.Add(ListPanel.MenuItem(Texts.CommonClearAll, viewModel.Clear, viewModel.Nodes.Count > 0, Texts.LauncherClearTip));
            });
        }

        private void AddEntries(LauncherSettingsViewModel viewModel)
        {
            var dialog = new LauncherAddDialog(viewModel.CreateCandidates()) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
                viewModel.AddNodes(dialog.SelectedNodes);
        }

        private void OnNodeDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBoxItem { IsSelected: true, DataContext: LauncherNodeRow row })
                return;

            row.Node.IsEditing = true;
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
            if (sender is FrameworkElement { DataContext: LauncherNodeRow row })
                row.Node.IsEditing = false;
        }

        private static LauncherNode? FindNode(object sender) =>
            ((sender as FrameworkElement)?.DataContext as LauncherNodeRow)?.Node;
    }
}
