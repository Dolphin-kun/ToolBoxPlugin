using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using YukkuriMovieMaker.Controls;
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
                return HasEditableProperties(settings) ? new PropertiesEditor { Target = settings, Width = 300 } : null;

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

        private static bool HasEditableProperties(ToolItemSettings settings) =>
            settings.GetType().GetProperties().Any(property => property.GetCustomAttribute<DisplayAttribute>() != null);
    }
}
