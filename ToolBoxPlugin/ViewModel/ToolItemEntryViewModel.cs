using System.Windows;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ToolItemEntryViewModel : Bindable
    {
        private readonly ToolBoxSettings settings;
        private readonly Type settingsType;
        private readonly string typeName;
        private readonly Type? settingsViewType;

        public string DisplayName { get; }
        public string IconPath { get; }

        public bool IsVisible
        {
            get => settings.Items.OfType<ToolItemSettings>().Any(x => x.GetType() == settingsType);
            set
            {
                var current = settings.Items.OfType<ToolItemSettings>().FirstOrDefault(x => x.GetType() == settingsType);
                if (value && current == null)
                {
                    Point pos;
                    if (settings.HiddenItemPositions.TryGetValue(typeName, out var savedPos))
                    {
                        bool occupied = settings.Items.Any(i => i.Position == savedPos);
                        pos = occupied ? FindEmptyPosition() : savedPos;
                        settings.HiddenItemPositions.Remove(typeName);
                    }
                    else
                    {
                        pos = FindEmptyPosition();
                    }

                    var newItem = (ToolItemSettings)Activator.CreateInstance(settingsType)!;
                    newItem.Position = pos;
                    settings.Items.Add(newItem);
                }
                else if (!value && current != null)
                {
                    settings.HiddenItemPositions[typeName] = current.Position;
                    settings.Items.Remove(current);
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(SettingsInstance));
                OnPropertyChanged(nameof(HasSettingsView));
                OnPropertyChanged(nameof(SettingsView));
            }
        }

        public ToolItemSettings? SettingsInstance =>
            settings.Items.OfType<ToolItemSettings>().FirstOrDefault(x => x.GetType() == settingsType);

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

        public ToolItemEntryViewModel(ToolBoxSettings settingsObj, Type settingsTypeObj, string displayName, string iconPath, Type? settingsViewTypeObj = null)
        {
            settings = settingsObj;
            settingsType = settingsTypeObj;
            typeName = settingsType.FullName ?? settingsType.Name;
            settingsViewType = settingsViewTypeObj;
            DisplayName = displayName;
            IconPath = iconPath;

            settings.Items.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsVisible));
                OnPropertyChanged(nameof(SettingsInstance));
                OnPropertyChanged(nameof(HasSettingsView));
                OnPropertyChanged(nameof(SettingsView));
            };
        }

        private Point FindEmptyPosition()
        {
            var occupied = settings.Items.Select(i => i.Position).ToHashSet();
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
