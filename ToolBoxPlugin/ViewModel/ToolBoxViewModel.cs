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

        /// <summary>
        /// 生存しているパネルのいずれか。ToolItemSettings.DefaultCommand から参照される。
        /// コマンドはどのパネルのものでも同じタイムラインに対して動くため、最後に開かれたパネルを返す。
        /// </summary>
        public static ToolBoxViewModel? Instance => instances.Count > 0 ? instances[^1] : null;

        private readonly DisposeCollector disposer = new();
        public DisposeCollector Disposer => disposer;

        /// <summary>全パネル共通の設定 (色・セルサイズなど)。</summary>
        public static ToolBoxSettings ToolBoxSettings => ToolBoxSettings.Default;

        /// <summary>このパネル固有の状態 (アイテム配置・グリッド数)。</summary>
        public ToolBoxPanel Panel { get; }

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

        /// <summary>ツールウィンドウのタイトル。パネル名が付いていれば「ツールボックス / 名前」になる。</summary>
        public string Title => string.IsNullOrWhiteSpace(Panel.PanelName)
            ? BaseTitle
            : $"{BaseTitle} / {Panel.PanelName}";

        // CanSuspend は IToolViewModel の既定実装 (true) をそのまま使う。
        // false にするとツールウィンドウを閉じられなくなる。

        /// <summary>新しいパネルを開くよう YMM4 へ要求するためのイベント。</summary>
        public event EventHandler<CreateNewToolViewRequestedEventArgs>? CreateNewToolViewRequested;

        public ToolState SaveState() => new()
        {
            Title = Title,
            SavedState = Panel.ToJson(),
        };

        /// <summary>
        /// 空のパネルを新しく開く。アイテムは引き継がず、グリッド数と見た目だけ現在のパネルに合わせる。
        /// </summary>
        public void AddPanel()
        {
            var newPanel = new ToolBoxPanel
            {
                GridRowCount = Panel.GridRowCount,
                GridColumnCount = Panel.GridColumnCount,
            };

            CreateNewToolViewRequested?.Invoke(this, new CreateNewToolViewRequestedEventArgs(new ToolState
            {
                Title = Title,
                SavedState = newPanel.ToJson(),
            }));
        }

        public void LoadState(ToolState stateData)
        {
            // タイトルは Panel.PanelName から組み立てるので、状態は SavedState だけ復元すればよい
            if (!string.IsNullOrEmpty(stateData.SavedState))
                Panel.LoadJson(stateData.SavedState);

            RegisterNewAddonItems();
            RemoveOrphanedPlacedItems();
        }

        #endregion

        public ToolBoxViewModel()
        {
            instances.Add(this);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            ToolBoxSettings.Default.Migrate();

            // アドオンが提供する設定型を復元できるよう、パネルを組み立てる前に読み込んでおく
            AddonLoader.LoadAddons();
            Panel = ToolBoxPanel.CreateDefault();
            Panel.PropertyChanged += OnPanelPropertyChanged;

            foreach (var item in DiscoverDraggableItems())
            {
                RegisterItem(item);
            }

            // 前回起動時に配置され、その後アドオンが削除された残骸アイテムを掃除する
            RemoveOrphanedPlacedItems();

            AddonLoader.AddonsChanged += OnAddonsChanged;
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

        /// <summary>
        /// 実行中に追加/更新されたアドオンのアイテムをツールボックスへ反映する。
        /// 既に登録済みの型は触らないため、配置済みの既存アイテムには影響しない。
        /// </summary>
        private void RegisterNewAddonItems()
        {
            foreach (var item in AddonLoader.GetAllDraggableItems())
            {
                RegisterItem(item);
            }
        }

        /// <summary>
        /// アドオンが削除されて存在しなくなった SettingsType のアイテムをパネルから取り除く。
        /// 対応するアドオンが無くなったアイテムは操作も設定変更もできない残骸になるため。
        /// </summary>
        private void RemoveOrphanedPlacedItems()
        {
            var validTypes = new HashSet<Type>(
                typeof(DraggableItem).Assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(ToolItemSettings).IsAssignableFrom(t)));

            foreach (var settings in AddonLoader.GetAllSettings())
                validTypes.Add(settings.GetType());

            for (var i = Panel.Items.Count - 1; i >= 0; i--)
            {
                if (!validTypes.Contains(Panel.Items[i].GetType()))
                    Panel.Items.RemoveAt(i);
            }
        }

        /// <summary>
        /// アイテムのコマンドと DataTemplate を登録する。同じ SettingsType は一度しか登録しない。
        /// </summary>
        private void RegisterItem(DraggableItem item)
        {
            if (!registeredSettingsTypes.Add(item.SettingsType))
                return;

            RegisterCommands(item);
            RegisterDataTemplate(item);
        }

        private static List<DraggableItem> DiscoverDraggableItems()
        {
            var items = new List<DraggableItem>();

            foreach (var item in typeof(DraggableItem).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(DraggableItem).IsAssignableFrom(t))
                .Select(t => (DraggableItem)Activator.CreateInstance(t)!))
            {
                items.Add(item);
            }

            items.AddRange(AddonLoader.GetAllDraggableItems());
            return items;
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
            timeline?.PropertyChanged -= Timeline_PropertyChanged;
            disposer.Dispose();
            instances.Remove(this);

            GC.SuppressFinalize(this);
        }
    }
}
