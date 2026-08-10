using System.Windows.Input;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    public sealed class PanelHotkeyRegistrar : IDisposable
    {
        private readonly ToolBoxPanel panel;
        private readonly PanelItemWatcher watcher;
        private readonly Dictionary<(Key Key, ModifierKeys Modifiers), ToolItemSettings> bindings = [];

        private bool isEnabled;

        public PanelHotkeyRegistrar(ToolBoxPanel panel)
        {
            this.panel = panel;
            watcher = new PanelItemWatcher(panel, [nameof(ToolItemSettings.HotkeyGesture)], Rebuild);
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

        private void Rebuild()
        {
            bindings.Clear();

            if (!isEnabled)
                return;

            foreach (var item in panel.Items)
            {
                var gesture = HotkeyGesture.ParseAssignable(item);
                if (gesture == null)
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

        public void Dispose()
        {
            Disable();
            watcher.Dispose();
        }
    }
}
