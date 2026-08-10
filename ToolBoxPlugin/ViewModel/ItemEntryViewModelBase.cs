using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public abstract class ItemEntryViewModelBase : Bindable
    {
        protected readonly ToolBoxPanel Panel;

        private bool isSyncing;

        protected ItemEntryViewModelBase(ToolBoxPanel panel)
        {
            Panel = panel;

            AddInstanceCommand = new ActionCommand(_ => true, _ => AddInstance());
            ResetAppearanceCommand = new ActionCommand(
                _ => SelectedInstance != null,
                _ => SelectedInstance?.ResetAppearance());
            RemoveInstanceCommand = new ActionCommand(
                param => param is ToolItemSettings,
                param =>
                {
                    if (param is ToolItemSettings settings)
                        RemoveInstance(settings);
                });

            Panel.Items.CollectionChanged += OnPanelItemsChanged;
        }

        protected void InitializeInstances() => SyncInstances();

        protected abstract bool Owns(ToolItemSettings settings);

        protected abstract ToolItemSettings CreateInstance();

        protected abstract string PositionKey { get; }

        public abstract string DisplayName { get; }
        public abstract string IconPath { get; }
        public abstract string Description { get; }

        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public ObservableCollection<ToolItemSettings> Instances { get; } = [];

        public int InstanceCount => Instances.Count;
        public bool HasInstances => Instances.Count > 0;

        public ICommand AddInstanceCommand { get; }
        public ICommand RemoveInstanceCommand { get; }
        public ICommand ResetAppearanceCommand { get; }

        private ToolItemSettings? selectedInstance;
        public ToolItemSettings? SelectedInstance
        {
            get => selectedInstance;
            set => Set(ref selectedInstance, value, etcChangedPropertyNames: [nameof(SettingsInstance), .. ExtraSelectionPropertyNames]);
        }

        public ToolItemSettings? SettingsInstance => SelectedInstance;

        public bool IsVisible
        {
            get => HasInstances;
            set
            {
                if (value == HasInstances)
                    return;

                if (value)
                {
                    AddInstance();
                }
                else
                {
                    foreach (var settings in Instances.ToList())
                        RemoveInstance(settings);
                }
            }
        }

        public void AddInstance()
        {
            var settings = CreateInstance();
            settings.Position = ResolveNewPosition();
            Panel.Items.Add(settings);
            SelectedInstance = settings;
        }

        public void RemoveInstance(ToolItemSettings settings)
        {
            if (!Panel.Items.Contains(settings))
                return;

            if (Instances.Count == 1)
                Panel.HiddenItemPositions[PositionKey] = settings.Position;

            Panel.Items.Remove(settings);
        }

        private Point ResolveNewPosition()
        {
            if (Panel.HiddenItemPositions.TryGetValue(PositionKey, out var saved))
            {
                Panel.HiddenItemPositions.Remove(PositionKey);
                if (!Panel.Items.Any(i => i.Position == saved))
                    return saved;
            }

            return FindEmptyPosition();
        }

        private Point FindEmptyPosition()
        {
            var occupied = Panel.Items.Select(i => i.Position).ToHashSet();
            var columns = Math.Max(1, Panel.GridColumnCount);
            var rows = Math.Max(1, Panel.GridRowCount);

            for (var y = 0; y < rows; y++)
                for (var x = 0; x < columns; x++)
                {
                    var candidate = new Point(x, y);
                    if (!occupied.Contains(candidate))
                        return candidate;
                }

            return new Point(0, 0);
        }

        private void OnPanelItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => SyncInstances();

        private void SyncInstances()
        {
            if (isSyncing)
                return;

            isSyncing = true;
            try
            {
                var current = Panel.Items.Where(Owns).ToList();

                Instances.Clear();
                foreach (var item in current)
                    Instances.Add(item);

                if (selectedInstance == null || !current.Contains(selectedInstance))
                    SelectedInstance = current.FirstOrDefault();

                OnPropertyChanged(nameof(InstanceCount));
                OnPropertyChanged(nameof(HasInstances));
                OnPropertyChanged(nameof(IsVisible));
                OnPropertyChanged(nameof(SettingsInstance));

                foreach (var name in ExtraSelectionPropertyNames)
                    OnPropertyChanged(name);
            }
            finally
            {
                isSyncing = false;
            }
        }

        protected virtual string[] ExtraSelectionPropertyNames => [];
    }
}
