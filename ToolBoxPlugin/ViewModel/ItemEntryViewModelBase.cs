using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ToolBox.Commons;
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

        private string? positionKey;

        /// <summary>非表示にしたときの控え先。アイテム側の区分と必ず一致させるため見本から取る。</summary>
        protected string PositionKey => positionKey ??= CreateInstance().HiddenKey;

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
            set
            {
                selectedInstance?.PropertyChanged -= OnSelectedInstancePropertyChanged;

                Set(ref selectedInstance, value, etcChangedPropertyNames: [nameof(SettingsInstance), .. ExtraSelectionPropertyNames]);

                selectedInstance?.PropertyChanged += OnSelectedInstancePropertyChanged;

                UpdateHotkeyConflict();
            }
        }

        public ToolItemSettings? SettingsInstance => SelectedInstance;

        private string hotkeyConflictMessage = string.Empty;
        public string HotkeyConflictMessage
        {
            get => hotkeyConflictMessage;
            private set => Set(ref hotkeyConflictMessage, value, etcChangedPropertyNames: nameof(HasHotkeyConflict));
        }

        public bool HasHotkeyConflict => !string.IsNullOrEmpty(HotkeyConflictMessage);

        private void OnSelectedInstancePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ToolItemSettings.HotkeyGesture))
                UpdateHotkeyConflict();
        }

        /// <summary>
        /// 同じキーを割り当てているアイテムが他にあれば知らせる。
        /// 実行時は先に置かれている 1 つだけが反応するため、気付けないと原因が分かりにくい。
        /// </summary>
        private void UpdateHotkeyConflict()
        {
            var current = selectedInstance;
            var gesture = current == null ? null : HotkeyGesture.ParseAssignable(current);
            if (current == null || gesture == null)
            {
                HotkeyConflictMessage = string.Empty;
                return;
            }

            var conflicts = ToolBoxViewModel.GetAllPlacedItems()
                .Where(item => !ReferenceEquals(item, current))
                .Where(item => HotkeyGesture.ParseAssignable(item) is { } other
                               && other.Key == gesture.Key
                               && other.Modifiers == gesture.Modifiers)
                .Select(item => item.InstanceSummary)
                .Distinct()
                .ToList();

            HotkeyConflictMessage = conflicts.Count == 0
                ? string.Empty
                : $"「{string.Join("」「", conflicts)}」と重複しています。先に配置されている方だけが反応します。";
        }

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
            // 非表示にしただけのアイテムがあれば、設定を持ったまま戻す
            var restored = Panel.TakeHiddenItem(PositionKey);
            var settings = restored != null && Owns(restored) ? restored : CreateInstance();

            settings.Position = ResolveNewPosition(restored?.Position);
            Panel.Items.Add(settings);
            SelectedInstance = settings;
        }

        public void RemoveInstance(ToolItemSettings settings)
        {
            if (!Panel.Items.Contains(settings))
                return;

            // 最後の 1 個なら設定ごと控えておき、再表示したときに元の状態へ戻せるようにする
            if (Instances.Count == 1)
                Panel.HideItem(settings);
            else
                Panel.Items.Remove(settings);
        }

        private Point ResolveNewPosition(Point? preferred)
        {
            if (preferred is { } saved && !Panel.Items.Any(i => i.Position == saved))
                return saved;

            if (Panel.HiddenItemPositions.TryGetValue(PositionKey, out var remembered))
            {
                Panel.HiddenItemPositions.Remove(PositionKey);
                if (!Panel.Items.Any(i => i.Position == remembered))
                    return remembered;
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
                else
                    UpdateHotkeyConflict();

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
