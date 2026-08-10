using System.Windows;
using ToolBox.Items.Shortcuts;
using ToolBox.Items.Shortcuts.Settings;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ShortcutEntryViewModel : Bindable
    {
        private readonly ToolBoxPanel panel;
        private readonly ShortcutDefinition definition;
        private readonly string positionKey;

        public string DisplayName => definition.DisplayName;
        public string IconPath { get; }
        public string Description => definition.Description;
        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public bool IsVisible
        {
            get => CurrentSettings != null;
            set
            {
                var current = CurrentSettings;
                if (value && current == null)
                {
                    Point pos;
                    if (panel.HiddenItemPositions.TryGetValue(positionKey, out var savedPos))
                    {
                        bool occupied = panel.Items.Any(i => i.Position == savedPos);
                        pos = occupied ? FindEmptyPosition() : savedPos;
                        panel.HiddenItemPositions.Remove(positionKey);
                    }
                    else
                    {
                        pos = FindEmptyPosition();
                    }
                    
                    panel.Items.Add(new ShortcutSettings
                    {
                        Position = pos,
                        CommandType = definition.CommandType,
                        Label = definition.DefaultLabel
                    });
                }
                else if (!value && current != null)
                {
                    panel.HiddenItemPositions[positionKey] = current.Position;
                    panel.Items.Remove(current);
                }
                NotifyAll();
            }
        }

        public ShortcutSettings? CurrentSettings =>
            panel.Items.OfType<ShortcutSettings>()
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

        public ShortcutEntryViewModel(ToolBoxPanel panelObj, ShortcutDefinition definitionObj)
        {
            panel = panelObj;
            definition = definitionObj;
            positionKey = "Shortcut_" + definition.CommandType.ToString();
            IconPath = definition.IconPath;

            panel.Items.CollectionChanged += (_, _) => NotifyAll();
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
