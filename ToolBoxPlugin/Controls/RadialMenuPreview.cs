using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ToolBox.Commons;
using ToolBox.Settings;

namespace ToolBox.Controls
{
    public sealed class RadialMenuPreview : ContentControl
    {
        public static readonly DependencyProperty NodesProperty =
            DependencyProperty.Register(nameof(Nodes), typeof(IEnumerable<LauncherNode>), typeof(RadialMenuPreview),
                new PropertyMetadata(null, OnNodesChanged));

        public IEnumerable<LauncherNode>? Nodes
        {
            get => (IEnumerable<LauncherNode>?)GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        private readonly List<IReadOnlyList<RadialEntry>> path = [];

        private readonly List<INotifyPropertyChanged> watchedNodes = [];
        private readonly List<INotifyCollectionChanged> watchedCollections = [];

        private bool isRebuildScheduled;

        public RadialMenuPreview()
        {
            HorizontalContentAlignment = HorizontalAlignment.Center;
            VerticalContentAlignment = VerticalAlignment.Center;
            Loaded += (_, _) => Rebuild();
        }

        private static void OnNodesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RadialMenuPreview preview)
                preview.Rebuild();
        }

        private void ScheduleRebuild()
        {
            if (isRebuildScheduled)
                return;

            isRebuildScheduled = true;

            Dispatcher.BeginInvoke(() =>
            {
                isRebuildScheduled = false;
                Rebuild();
            });
        }

        private void Rebuild()
        {
            Unwatch();

            if (Nodes != null)
                Watch(Nodes);

            path.Clear();
            path.Add(BuildEntries());

            ShowCurrentLevel();
        }

        private IReadOnlyList<RadialEntry> BuildEntries()
        {
            var nodes = Nodes?.ToList() ?? [];

            if (nodes.Count == 0)
                return RadialEntryBuilder.BuildFromPanel();

            var entries = RadialEntryBuilder.Build(nodes);
            return entries.Count > 0 ? entries : RadialEntryBuilder.BuildFromPanel();
        }

        private void ShowCurrentLevel()
        {
            if (path.Count == 0)
                return;

            var entries = path[^1];

            if (entries.Count == 0)
            {
                Content = new TextBlock
                {
                    Text = "並べるアイテムがありません。",
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = SystemColors.GrayTextBrush,
                };
                return;
            }

            var menu = new RadialMenu(entries, hasParent: path.Count > 1, showAvailability: false);
            menu.EntryChosen += OnEntryChosen;
            menu.BackRequested += GoBack;

            Content = new Viewbox
            {
                Child = menu,
                Stretch = System.Windows.Media.Stretch.Uniform,
                MaxWidth = RadialMenu.OuterRadius * 2,
            };
        }

        private void OnEntryChosen(RadialEntry entry)
        {
            if (!entry.IsFolder)
                return;

            path.Add(entry.Children);
            ShowCurrentLevel();
        }

        private void GoBack()
        {
            if (path.Count <= 1)
                return;

            path.RemoveAt(path.Count - 1);
            ShowCurrentLevel();
        }

        private void Watch(IEnumerable<LauncherNode> nodes)
        {
            if (nodes is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += OnWatchedChanged;
                watchedCollections.Add(collection);
            }

            foreach (var node in nodes)
            {
                node.PropertyChanged += OnWatchedPropertyChanged;
                watchedNodes.Add(node);

                Watch(node.Children);
            }
        }

        private void Unwatch()
        {
            foreach (var node in watchedNodes)
                node.PropertyChanged -= OnWatchedPropertyChanged;

            foreach (var collection in watchedCollections)
                collection.CollectionChanged -= OnWatchedChanged;

            watchedNodes.Clear();
            watchedCollections.Clear();
        }

        private void OnWatchedChanged(object? sender, NotifyCollectionChangedEventArgs e) => ScheduleRebuild();

        private void OnWatchedPropertyChanged(object? sender, PropertyChangedEventArgs e) => ScheduleRebuild();
    }
}
