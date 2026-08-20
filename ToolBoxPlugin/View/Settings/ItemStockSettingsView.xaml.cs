using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.View.Settings
{
    public partial class ItemStockSettingsView : UserControl
    {
        public ItemStockSettingsView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
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
