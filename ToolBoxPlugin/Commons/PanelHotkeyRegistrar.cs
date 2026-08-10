using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    public sealed class PanelHotkeyRegistrar : IDisposable
    {
        private readonly ToolBoxPanel panel;
        private readonly HashSet<ToolItemSettings> watchedItems = [];

        private readonly Dictionary<(Key Key, ModifierKeys Modifiers), ToolItemSettings> bindings = [];

        private bool isEnabled;

        public PanelHotkeyRegistrar(ToolBoxPanel panel)
        {
            this.panel = panel;
            panel.Items.CollectionChanged += OnItemsChanged;
            WatchAllItems();
        }

        public void Enable()
        {
            if (isEnabled)
                return;

            isEnabled = true;
            PanelKeyRouter.Add(OnKey);
            Rebuild();
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            isEnabled = false;
            PanelKeyRouter.Remove(OnKey);
            bindings.Clear();
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            WatchAllItems();
            Rebuild();
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ToolItemSettings.HotkeyGesture))
                Rebuild();
        }

        private void WatchAllItems()
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

        private void Rebuild()
        {
            bindings.Clear();

            if (!isEnabled)
                return;

            foreach (var item in panel.Items)
            {
                if (!item.IsInvokable || string.IsNullOrWhiteSpace(item.HotkeyGesture))
                    continue;

                var gesture = HotkeyGesture.Parse(item.HotkeyGesture);
                if (gesture == null || !HasRequiredModifier(gesture.Modifiers))
                    continue;

                bindings.TryAdd((gesture.Key, gesture.Modifiers), item);
            }
        }

        private bool OnKey(Key key, ModifierKeys modifiers)
        {
            if (!bindings.TryGetValue((key, modifiers), out var item))
                return false;

            var command = item.DefaultCommand;
            if (command?.CanExecute(item) != true)
                return false;

            command.Execute(item);
            return true;
        }

        private static bool HasRequiredModifier(ModifierKeys modifiers) =>
            !modifiers.HasFlag(ModifierKeys.Windows)
            && (modifiers.HasFlag(ModifierKeys.Control) || modifiers.HasFlag(ModifierKeys.Alt));

        public void Dispose()
        {
            Disable();
            panel.Items.CollectionChanged -= OnItemsChanged;
            foreach (var item in watchedItems)
                item.PropertyChanged -= OnItemPropertyChanged;
            watchedItems.Clear();
        }
    }
}
