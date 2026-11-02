using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using ToolBox.Controls;
using ToolBox.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Resources.Icons;

namespace ToolBox.View.Settings
{
    public partial class ItemStockSettingsView : UserControl
    {
        public ItemStockSettingsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;

            var list = StockList.ListBox!;
            list.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(ItemStockSettingsViewModel.Stocks)));
            list.SetBinding(Selector.SelectedItemProperty, new Binding(nameof(ItemStockSettingsViewModel.SelectedStock)) { Mode = BindingMode.TwoWay });
            list.ItemTemplate = (DataTemplate)FindResource("StockTemplate");
            StockList.ItemContainerSetters.Add(new EventSetter(MouseDoubleClickEvent, new MouseButtonEventHandler(OnStockDoubleClick)));

            DataContextChanged += (_, _) => BuildButtons();
        }

        private void BuildButtons()
        {
            if (ViewModel is not { } viewModel || StockList.Tag == viewModel)
                return;

            StockList.Tag = viewModel;
            StockList.ClearButtons();
            StockList.AddCommandButton(GeometryKeys.Plus, Texts.ItemStockSaveTip, viewModel.AddCommand);
            StockList.AddCommandButton(GeometryKeys.Minus, Texts.CommonDelete, viewModel.RemoveCommand);
            StockList.AddCommandButton(GeometryKeys.MenuUp, Texts.CommonMoveUp, viewModel.MoveUpCommand);
            StockList.AddCommandButton(GeometryKeys.MenuDown, Texts.CommonMoveDown, viewModel.MoveDownCommand);
            StockList.AddMenuButton(GeometryKeys.DotsVertical, Texts.CommonMoreActions, menu =>
            {
                menu.Items.Add(ListPanel.MenuItem(Texts.CommonRename, () => viewModel.SelectedStock?.IsEditing = true, viewModel.SelectedStock != null));
                menu.Items.Add(ListPanel.MenuItem(Texts.CommonOverwrite, () => viewModel.OverwriteCommand.Execute(null), viewModel.OverwriteCommand.CanExecute(null), Texts.ItemStockOverwriteTip));
                menu.Items.Add(ListPanel.MenuItem(Texts.CommonClearAll, () => viewModel.ClearCommand.Execute(null), viewModel.ClearCommand.CanExecute(null)));
            });
        }

        private ItemStockSettingsViewModel? ViewModel => DataContext as ItemStockSettingsViewModel;

        private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel?.RefreshCommands();

        private void OnStockDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListBoxItem { IsSelected: true, DataContext: ItemStockEntry entry })
                return;

            entry.IsEditing = true;
            e.Handled = true;
        }

        private void OnRenameClick(object sender, RoutedEventArgs e)
        {
            if (FindEntry(sender) is { } entry)
                entry.IsEditing = true;
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (FindEntry(sender) is { } entry)
                ViewModel?.Remove(entry);
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
            if (sender is FrameworkElement { DataContext: ItemStockEntry entry })
                entry.IsEditing = false;
        }

        private static ItemStockEntry? FindEntry(object sender) =>
            (sender as FrameworkElement)?.DataContext as ItemStockEntry;
    }
}
