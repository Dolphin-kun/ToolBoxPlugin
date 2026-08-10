using System.Windows;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ToolItemEntryViewModel : Bindable
    {
        private readonly ToolBoxPanel panel;
        private readonly Type settingsType;
        private readonly string typeName;
        private readonly Type? settingsViewType;

        public string DisplayName { get; }
        public string IconPath { get; }
        public string Description { get; }
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public bool IsVisible
        {
            get => panel.Items.OfType<ToolItemSettings>().Any(x => x.GetType() == settingsType);
            set
            {
                var current = panel.Items.OfType<ToolItemSettings>().FirstOrDefault(x => x.GetType() == settingsType);
                if (value && current == null)
                {
                    Point pos;
                    if (panel.HiddenItemPositions.TryGetValue(typeName, out var savedPos))
                    {
                        bool occupied = panel.Items.Any(i => i.Position == savedPos);
                        pos = occupied ? FindEmptyPosition() : savedPos;
                        panel.HiddenItemPositions.Remove(typeName);
                    }
                    else
                    {
                        pos = FindEmptyPosition();
                    }

                    var newItem = (ToolItemSettings)Activator.CreateInstance(settingsType)!;
                    newItem.Position = pos;
                    panel.Items.Add(newItem);
                }
                else if (!value && current != null)
                {
                    panel.HiddenItemPositions[typeName] = current.Position;
                    panel.Items.Remove(current);
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(SettingsInstance));
                OnPropertyChanged(nameof(HasSettingsView));
                OnPropertyChanged(nameof(SettingsView));
            }
        }

        public ToolItemSettings? SettingsInstance =>
            panel.Items.OfType<ToolItemSettings>().FirstOrDefault(x => x.GetType() == settingsType);

        public bool HasSettingsView => settingsViewType != null && IsVisible;

        public UIElement? SettingsView
        {
            get
            {
                if (settingsViewType == null || !IsVisible || SettingsInstance == null) return null;
                try
                {
                    var view = (UIElement)Activator.CreateInstance(settingsViewType)!;
                    if (view is FrameworkElement fe)
                        fe.DataContext = SettingsInstance;
                    return view;
                }
                catch { return null; }
            }
        }

        public ToolItemEntryViewModel(ToolBoxPanel panelObj, Type settingsTypeObj, string displayName, string iconPath, string description, Type? settingsViewTypeObj = null)
        {
            panel = panelObj;
            settingsType = settingsTypeObj;
            typeName = settingsType.FullName ?? settingsType.Name;
            settingsViewType = settingsViewTypeObj;
            DisplayName = displayName;
            IconPath = iconPath;
            Description = description;

            panel.Items.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsVisible));
                OnPropertyChanged(nameof(SettingsInstance));
                OnPropertyChanged(nameof(HasSettingsView));
                OnPropertyChanged(nameof(SettingsView));
            };
        }

        private Point FindEmptyPosition()
        {
            var occupied = panel.Items.Select(i => i.Position).ToHashSet();
            for (int y = 0; y < 10; y++)
                for (int x = 0; x < 6; x++)
                {
                    var p = new Point(x, y);
                    if (!occupied.Contains(p)) return p;
                }
            return new Point(0, 0);
        }
    }
}
