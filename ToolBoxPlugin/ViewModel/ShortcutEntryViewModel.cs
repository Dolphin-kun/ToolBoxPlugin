using System.Windows;
using ToolBox.Items.Shortcuts;
using ToolBox.Items.Shortcuts.Settings;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ShortcutEntryViewModel : Bindable
    {
        private readonly ToolBoxSettings settings;
        private readonly ShortcutDefinition definition;
        private readonly string positionKey;

        public string DisplayName => definition.DisplayName;
        public string IconPath { get; }

        public bool IsVisible
        {
            get => CurrentSettings != null;
            set
            {
                var current = CurrentSettings;
                if (value && current == null)
                {
                    Point pos;
                    if (settings.HiddenItemPositions.TryGetValue(positionKey, out var savedPos))
                    {
                        bool occupied = settings.Items.Any(i => i.Position == savedPos);
                        pos = occupied ? FindEmptyPosition() : savedPos;
                        settings.HiddenItemPositions.Remove(positionKey);
                    }
                    else
                    {
                        pos = FindEmptyPosition();
                    }
                    
                    settings.Items.Add(new ShortcutSettings
                    {
                        Position = pos,
                        CommandType = definition.CommandType,
                        Label = definition.DefaultLabel
                    });
                }
                else if (!value && current != null)
                {
                    settings.HiddenItemPositions[positionKey] = current.Position;
                    settings.Items.Remove(current);
                }
                NotifyAll();
            }
        }

        public ShortcutSettings? CurrentSettings =>
            settings.Items.OfType<ShortcutSettings>()
                .FirstOrDefault(s => s.CommandType == definition.CommandType);

        public ToolItemSettings? SettingsInstance => CurrentSettings;

        public bool CanEditLabel => IsVisible;

        public string Label
        {
            get => CurrentSettings?.Label ?? definition.DefaultLabel;
            set
            {
                CurrentSettings?.Label = value;
            }
        }

        public ShortcutEntryViewModel(ToolBoxSettings settingsObj, ShortcutDefinition definitionObj)
        {
            settings = settingsObj;
            definition = definitionObj;
            positionKey = "Shortcut_" + definition.CommandType.ToString();
            IconPath = definition.IconPath;

            settings.Items.CollectionChanged += (_, _) => NotifyAll();
        }

        private void NotifyAll()
        {
            OnPropertyChanged(nameof(IsVisible));
            OnPropertyChanged(nameof(CurrentSettings));
            OnPropertyChanged(nameof(CanEditLabel));
            OnPropertyChanged(nameof(Label));
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
