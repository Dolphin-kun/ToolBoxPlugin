using System.Collections.Specialized;
using System.ComponentModel;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    public sealed class PanelItemWatcher : IDisposable
    {
        private readonly ToolBoxPanel panel;
        private readonly HashSet<string> watchedProperties;
        private readonly Action onChanged;
        private readonly HashSet<ToolItemSettings> watchedItems = [];

        public PanelItemWatcher(ToolBoxPanel panel, IEnumerable<string> watchedProperties, Action onChanged)
        {
            this.panel = panel;
            this.watchedProperties = [.. watchedProperties];
            this.onChanged = onChanged;

            panel.Items.CollectionChanged += OnItemsChanged;
            Rewatch();
        }

        public void Rewatch()
        {
            foreach (var item in watchedItems)
                item.PropertyChanged -= OnItemPropertyChanged;

            watchedItems.Clear();

            foreach (var item in panel.Items)
            {
                item.PropertyChanged += OnItemPropertyChanged;
                watchedItems.Add(item);
            }
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Rewatch();
            onChanged();
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null && watchedProperties.Contains(e.PropertyName))
                onChanged();
        }

        public void Dispose()
        {
            panel.Items.CollectionChanged -= OnItemsChanged;
            foreach (var item in watchedItems)
                item.PropertyChanged -= OnItemPropertyChanged;
            watchedItems.Clear();
        }
    }
}
