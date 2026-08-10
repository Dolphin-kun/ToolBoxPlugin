using System.Windows;
using ToolBox.Settings;

namespace ToolBox.ViewModel
{
    public class ToolItemEntryViewModel : ItemEntryViewModelBase
    {
        private readonly Type settingsType;
        private readonly Type? settingsViewType;

        public override string DisplayName { get; }
        public override string IconPath { get; }
        public override string Description { get; }

        protected override string PositionKey { get; }

        public ToolItemEntryViewModel(
            ToolBoxPanel panel,
            Type settingsType,
            string displayName,
            string iconPath,
            string description,
            Type? settingsViewType = null)
            : base(panel)
        {
            this.settingsType = settingsType;
            this.settingsViewType = settingsViewType;
            PositionKey = settingsType.FullName ?? settingsType.Name;
            DisplayName = displayName;
            IconPath = iconPath;
            Description = description;

            InitializeInstances();
        }

        protected override bool Owns(ToolItemSettings settings) => settings.GetType() == settingsType;

        protected override ToolItemSettings CreateInstance() => (ToolItemSettings)Activator.CreateInstance(settingsType)!;

        protected override string[] ExtraSelectionPropertyNames => [nameof(HasSettingsView), nameof(SettingsView)];

        public bool HasSettingsView => settingsViewType != null && SelectedInstance != null;

        public UIElement? SettingsView
        {
            get
            {
                if (settingsViewType == null || SelectedInstance == null)
                    return null;

                try
                {
                    var view = (UIElement)Activator.CreateInstance(settingsViewType)!;
                    if (view is FrameworkElement fe)
                        fe.DataContext = SelectedInstance;
                    return view;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create settings view {settingsViewType}: {ex}");
                    return null;
                }
            }
        }
    }
}
