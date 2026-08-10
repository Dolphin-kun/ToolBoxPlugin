using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Controls;
using ToolBox.Items;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.ViewModel
{
    public class ToolBoxViewModel : Bindable, ITimelineToolViewModel, IToolViewModel, IToolBoxContext, IDisposable
    {
        private static readonly List<ToolBoxViewModel> instances = [];

        public static ToolBoxViewModel? Instance => instances.Count > 0 ? instances[^1] : null;

        private readonly DisposeCollector disposer = new();
        public DisposeCollector Disposer => disposer;

        public static ToolBoxSettings ToolBoxSettings => ToolBoxSettings.Default;

        public ToolBoxPanel Panel { get; }

        public PanelHistory History { get; }

        private readonly PanelHotkeyRegistrar hotkeyRegistrar;

        public ICommand UndoLayoutCommand { get; }
        public ICommand RedoLayoutCommand { get; }

        public void EnableHotkeys() => hotkeyRegistrar.Enable();

        private bool isSettingsVisible = false;
        public bool IsSettingsVisible
        {
            get => isSettingsVisible;
            set => Set(ref isSettingsVisible, value);
        }

        private ToolBoxSettingsViewModel? settingsViewModel;
        public ToolBoxSettingsViewModel SettingsViewModel
        {
            get
            {
                settingsViewModel ??= new ToolBoxSettingsViewModel(
                        ToolBoxSettings,
                        Panel,
                        () => IsSettingsVisible = false
                    );
                return settingsViewModel;
            }
        }

        private Timeline? timeline;
        private UndoRedoManager? undoRedoManager;
        private IReadOnlyList<IItem>? selectedItems;

        IReadOnlyList<IItem>? IToolBoxContext.SelectedItems => selectedItems;
        Timeline? IToolBoxContext.Timeline => timeline;
        UndoRedoManager? IToolBoxContext.UndoRedoManager => undoRedoManager;

        T? IToolBoxContext.FindItemSettings<T>() where T : class => Panel.Items.OfType<T>().FirstOrDefault();

        public Dictionary<string, ICommand> Commands { get; } = [];

        private readonly List<ActionCommand> allCommands = [];
        private readonly HashSet<Type> registeredSettingsTypes = [];

        #region IToolViewModel

        private const string BaseTitle = "ツールボックス";

        public string Title => string.IsNullOrWhiteSpace(Panel.PanelName)
            ? BaseTitle
            : $"{BaseTitle} / {Panel.PanelName}";

        public event EventHandler<CreateNewToolViewRequestedEventArgs>? CreateNewToolViewRequested;

        public ToolState SaveState() => new()
        {
            Title = Title,
            SavedState = Panel.ToJson(),
        };

        public void AddPanel()
        {
            var newPanel = new ToolBoxPanel
            {
                GridRowCount = Panel.GridRowCount,
                GridColumnCount = Panel.GridColumnCount,
            };

            RequestNewPanel(newPanel);
        }

        public void DuplicatePanel()
        {
            var copy = new ToolBoxPanel();
            copy.LoadJson(Panel.ToJson());
            copy.PanelName = BuildCopyName(Panel.PanelName);

            RequestNewPanel(copy);
        }

        private static string BuildCopyName(string original) =>
            string.IsNullOrWhiteSpace(original) ? "コピー" : $"{original} のコピー";

        private void RequestNewPanel(ToolBoxPanel newPanel)
        {
            CreateNewToolViewRequested?.Invoke(this, new CreateNewToolViewRequestedEventArgs(new ToolState
            {
                Title = Title,
                SavedState = newPanel.ToJson(),
            }));
        }

        public void LoadState(ToolState stateData)
        {
            if (!string.IsNullOrEmpty(stateData.SavedState))
                Panel.LoadJson(stateData.SavedState);

            RegisterNewAddonItems();
            RemoveOrphanedPlacedItems();

            History.Reset();
        }

        #endregion

        public ToolBoxViewModel()
        {
            instances.Add(this);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            ToolBoxSettings.Default.Migrate();

            AddonLoader.LoadAddons();
            Panel = ToolBoxPanel.CreateDefault();
            Panel.PropertyChanged += OnPanelPropertyChanged;

            foreach (var item in ItemCatalog.GetDraggableItems())
            {
                RegisterItem(item);
            }

            RemoveOrphanedPlacedItems();

            History = new PanelHistory(Panel);
            hotkeyRegistrar = new PanelHotkeyRegistrar(Panel);

            UndoLayoutCommand = new ActionCommand(_ => History.CanUndo, _ => History.Undo());
            RedoLayoutCommand = new ActionCommand(_ => History.CanRedo, _ => History.Redo());
            History.PropertyChanged += OnHistoryChanged;

            AddonLoader.AddonsChanged += OnAddonsChanged;
        }

        private void OnHistoryChanged(object? sender, PropertyChangedEventArgs e)
        {
            (UndoLayoutCommand as ActionCommand)?.RaiseCanExecuteChanged();
            (RedoLayoutCommand as ActionCommand)?.RaiseCanExecuteChanged();
        }

        private void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ToolBoxPanel.PanelName))
                OnPropertyChanged(nameof(Title));
        }

        private void OnAddonsChanged(object? sender, EventArgs e)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(HandleAddonsChanged);
                return;
            }

            HandleAddonsChanged();
        }

        private void HandleAddonsChanged()
        {
            RegisterNewAddonItems();
            RemoveOrphanedPlacedItems();
        }

        private void RegisterNewAddonItems()
        {
            foreach (var item in AddonLoader.GetAllDraggableItems())
            {
                RegisterItem(item);
            }
        }

        private void RemoveOrphanedPlacedItems()
        {
            var validTypes = ItemCatalog.GetKnownSettingsTypes();

            for (var i = Panel.Items.Count - 1; i >= 0; i--)
            {
                if (!validTypes.Contains(Panel.Items[i].GetType()))
                    Panel.Items.RemoveAt(i);
            }
        }

        private void RegisterItem(DraggableItem item)
        {
            if (!registeredSettingsTypes.Add(item.SettingsType))
                return;

            RegisterCommands(item);
            RegisterDataTemplate(item);
        }

        private void RegisterCommands(DraggableItem item)
        {
            foreach (var def in item.GetCommandDefinitions())
            {
                var capturedDef = def;
                var cmd = new ActionCommand(
                    param => capturedDef.CanExecute(this, param),
                    param => capturedDef.Execute(this, param));
                Commands[capturedDef.Name] = cmd;
                allCommands.Add(cmd);
            }
        }

        private static void RegisterDataTemplate(DraggableItem item)
        {
            try
            {
                var viewType = item.ViewType;
                var viewModelType = item.ViewModelType;
                var settingsType = item.SettingsType;

                var factory = new FrameworkElementFactory(viewType);

                if (viewModelType != null)
                {
                    factory.SetValue(AutoWireViewModel.ViewModelTypeProperty, viewModelType);
                }

                foreach (var def in item.GetCommandDefinitions())
                {
                    var pName = def.Name;
                    var dpField = viewType.GetField(pName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (dpField?.GetValue(null) is not DependencyProperty dp) continue;

                    var selfBinding = new Binding($"DataContext.Commands[{pName}]") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) };
                    var ancestorBinding = new Binding($"DataContext.Commands[{pName}]") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DraggableCanvas), 1) };

                    var priority = new PriorityBinding();
                    priority.Bindings.Add(selfBinding);
                    priority.Bindings.Add(ancestorBinding);

                    factory.SetBinding(dp, priority);
                }

                var dataTemplate = new DataTemplate { DataType = settingsType, VisualTree = factory };
                var key = new DataTemplateKey(settingsType);
                Application.Current.Resources[key] = dataTemplate;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to register data template for {item.GetType()}: {ex}");
            }
        }

        public void SetTimelineToolInfo(TimelineToolInfo info)
        {
            timeline?.PropertyChanged -= Timeline_PropertyChanged;
            UpdateSelectedItems(null);

            timeline = info.Timeline;
            undoRedoManager = info.UndoRedoManager;

            if (timeline != null)
            {
                timeline.PropertyChanged += Timeline_PropertyChanged;
                UpdateSelectedItems(timeline.SelectedItems);
            }
        }

        private void Timeline_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Timeline.SelectedItems))
                UpdateSelectedItems(timeline?.SelectedItems);
        }

        private void UpdateSelectedItems(IReadOnlyList<IItem>? newItems)
        {
            selectedItems = newItems;
            RaiseAllCanExecuteChanged();
        }

        private void RaiseAllCanExecuteChanged()
        {
            foreach (var cmd in allCommands)
                cmd.RaiseCanExecuteChanged();
        }

        public void Dispose()
        {
            AddonLoader.AddonsChanged -= OnAddonsChanged;
            Panel.PropertyChanged -= OnPanelPropertyChanged;
            History.PropertyChanged -= OnHistoryChanged;
            History.Dispose();
            hotkeyRegistrar.Dispose();
            timeline?.PropertyChanged -= Timeline_PropertyChanged;
            disposer.Dispose();
            instances.Remove(this);

            GC.SuppressFinalize(this);
        }
    }
}
