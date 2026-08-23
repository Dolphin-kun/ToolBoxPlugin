using System.Windows;
using ToolBox.Settings;

namespace ToolBox.Commons
{
    public static class ItemSettingsPanel
    {
        public static FrameworkElement? Create(object? dataContext)
        {
            if (dataContext is not ToolItemSettings settings)
                return null;

            if (!ItemCatalog.GetDraggableItemsBySettingsType().TryGetValue(settings.GetType(), out var item))
                return null;

            if (item.SettingsViewType is not { } viewType)
                return null;

            try
            {
                var panel = (FrameworkElement)Activator.CreateInstance(viewType)!;
                panel.DataContext = settings;
                return panel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create the settings panel for {settings.GetType()}: {ex}");
                return null;
            }
        }
    }
}
