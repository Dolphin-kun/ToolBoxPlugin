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
    public class ToolBoxViewModel : Bindable, ITimelineToolViewModel, IToolBoxContext, IDisposable
    {
        public static ToolBoxViewModel? Instance { get; private set; }

        private readonly DisposeCollector disposer = new();
        public DisposeCollector Disposer => disposer;

        public static ToolBoxSettings ToolBoxSettings => ToolBoxSettings.Default;

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

        public Dictionary<string, ICommand> Commands { get; } = [];

        private readonly List<ActionCommand> allCommands = [];

        public ToolBoxViewModel()
        {
            Instance = this;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            ToolBoxSettings.Default.Migrate();

            var draggableItems = DiscoverDraggableItems();

            foreach (var item in draggableItems)
            {
                RegisterCommands(item);
                RegisterDataTemplate(item);
            }
        }

        private static List<DraggableItem> DiscoverDraggableItems()
        {
            return [.. typeof(DraggableItem).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(DraggableItem).IsAssignableFrom(t))
                .Select(t => (DraggableItem)Activator.CreateInstance(t)!)];
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
            catch { }
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
            disposer.Dispose();
            if (Instance == this)
            {
                Instance = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
