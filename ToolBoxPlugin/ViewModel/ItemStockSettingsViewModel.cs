using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ItemStockSettingsViewModel : SettingsNodeViewModel
    {
        private static string Caption => Texts.ItemStockTitle;

        public ObservableCollection<ItemStockEntry> Stocks => ItemStockStore.Entries;

        private ItemStockEntry? selectedStock;
        public ItemStockEntry? SelectedStock
        {
            get => selectedStock;
            set
            {
                if (!Set(ref selectedStock, value))
                    return;

                RaiseSelectionCommands();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand OverwriteCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand ClearCommand { get; }

        public ItemStockSettingsViewModel() : base(Texts.ItemStockTitle)
        {
            AddCommand = new ActionCommand(_ => HasSelection(), _ => Add());
            OverwriteCommand = new ActionCommand(_ => HasSelection() && SelectedStock != null, _ => Overwrite());
            RemoveCommand = new ActionCommand(_ => SelectedStock != null, _ => Remove());
            MoveUpCommand = new ActionCommand(_ => SelectedStock != null, _ => Move(-1));
            MoveDownCommand = new ActionCommand(_ => SelectedStock != null, _ => Move(1));
            ClearCommand = new ActionCommand(_ => Stocks.Count > 0, _ => Clear());

            Stocks.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasStocks));
                (ClearCommand as ActionCommand)?.RaiseCanExecuteChanged();
            };
        }

        public bool HasStocks => Stocks.Count > 0;

        public void RefreshCommands()
        {
            (AddCommand as ActionCommand)?.RaiseCanExecuteChanged();
            RaiseSelectionCommands();
        }

        private void RaiseSelectionCommands()
        {
            (OverwriteCommand as ActionCommand)?.RaiseCanExecuteChanged();
            (RemoveCommand as ActionCommand)?.RaiseCanExecuteChanged();
            (MoveUpCommand as ActionCommand)?.RaiseCanExecuteChanged();
            (MoveDownCommand as ActionCommand)?.RaiseCanExecuteChanged();
        }

        private static IToolBoxContext? Context => ToolBoxViewModel.Instance;

        private static bool HasSelection() => Context?.SelectedItems?.Count > 0;

        private void Add()
        {
            var items = Context?.SelectedItems?.ToList();
            if (items is null || items.Count == 0)
            {
                MessageBox.Show(
                    Texts.ItemStockSelectFirst,
                    Caption,
                    MessageBoxButton.OK);
                return;
            }

            var entry = ItemStockStore.Create(items);
            if (entry == null)
            {
                MessageBox.Show(
                    Texts.ItemStockSaveFailed,
                    Caption,
                    MessageBoxButton.OK);
                return;
            }

            Stocks.Add(entry);
            SelectedStock = entry;
            entry.IsEditing = true;
        }

        private void Overwrite()
        {
            var items = Context?.SelectedItems?.ToList();
            if (items is null || items.Count == 0 || SelectedStock is not { } entry)
                return;

            var confirm = MessageBox.Show(
                string.Format(Texts.ItemStockOverwriteConfirm, entry.DisplayLabel),
                Caption,
                MessageBoxButton.OKCancel);

            if (confirm != MessageBoxResult.OK)
                return;

            if (!ItemStockStore.Overwrite(entry, items))
            {
                MessageBox.Show(
                    Texts.ItemStockSaveFailed,
                    Caption,
                    MessageBoxButton.OK);
            }
        }

        public void Remove(ItemStockEntry entry)
        {
            var users = CountUsers(entry);
            var message = users == 0
                ? string.Format(Texts.ItemStockDeleteConfirm, entry.DisplayLabel)
                : $"{string.Format(Texts.ItemStockDeleteConfirm, entry.DisplayLabel)}\n{string.Format(Texts.ItemStockDeleteConfirmUsers, users)}";

            var confirm = MessageBox.Show(message, Caption, MessageBoxButton.OKCancel);
            if (confirm != MessageBoxResult.OK)
                return;

            Stocks.Remove(entry);

            if (SelectedStock == entry)
                SelectedStock = null;

            ToolBoxViewModel.RefreshAllCommands();
        }

        private void Remove()
        {
            if (SelectedStock is { } entry)
                Remove(entry);
        }

        private static int CountUsers(ItemStockEntry entry) =>
            ToolBoxViewModel.GetAllPlacedItems()
                .OfType<Items.ItemPreset.Settings.ItemPresetSettings>()
                .Count(settings => settings.StockId == entry.Id);

        private void Move(int offset)
        {
            if (SelectedStock is not { } entry)
                return;

            var index = Stocks.IndexOf(entry);
            var newIndex = index + offset;

            if (index < 0 || newIndex < 0 || newIndex >= Stocks.Count)
                return;

            Stocks.Move(index, newIndex);
        }

        private void Clear()
        {
            var confirm = MessageBox.Show(
                $"{Texts.ItemStockClearConfirm}\n{Texts.ItemStockClearConfirmHint}",
                Caption,
                MessageBoxButton.OKCancel);

            if (confirm != MessageBoxResult.OK)
                return;

            Stocks.Clear();
            SelectedStock = null;
        }
    }
}
