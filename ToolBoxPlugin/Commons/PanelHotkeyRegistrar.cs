using System.Windows.Input;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    public sealed class PanelHotkeyRegistrar : IDisposable
    {
        private readonly ToolBoxPanel panel;
        private readonly PanelItemWatcher watcher;
        private readonly Dictionary<HotkeyTrigger, ToolItemSettings> bindings = [];

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
            PanelInputRouter.Add(OnInput);
            Rebuild();
        }

        public void Disable()
        {
            if (!isEnabled)
                return;

            isEnabled = false;
            PanelInputRouter.Remove(OnInput);
            bindings.Clear();
        }

        private void Rebuild()
        {
            bindings.Clear();

            if (!isEnabled)
                return;

            foreach (var item in panel.Items)
            {
                if (HotkeyGesture.ParseAssignable(item) is not { } trigger)
                    continue;

                bindings.TryAdd(trigger, item);
            }
        }

        private bool OnInput(HotkeyTrigger trigger)
        {
            if (!bindings.TryGetValue(trigger, out var item))
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
