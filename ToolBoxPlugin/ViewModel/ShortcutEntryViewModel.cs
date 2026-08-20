using ToolBox.Items.Shortcuts;
using ToolBox.Items.Shortcuts.Settings;
using ToolBox.Settings;

namespace ToolBox.ViewModel
{
    public class ShortcutEntryViewModel : ItemEntryViewModelBase
    {
        private readonly ShortcutDefinition definition;

        public override string DisplayName => definition.DisplayName;
        public override string IconPath { get; }
        public override string Description => definition.Description;

        public ShortcutEntryViewModel(ToolBoxPanel panel, ShortcutDefinition definition)
            : base(panel)
        {
            this.definition = definition;
            IconPath = definition.IconPath;

            InitializeInstances();
        }

        protected override bool Owns(ToolItemSettings settings) =>
            settings is ShortcutSettings shortcut && shortcut.CommandType == definition.CommandType;

        protected override ToolItemSettings CreateInstance() => new ShortcutSettings
        {
            CommandType = definition.CommandType,
            Label = definition.DefaultLabel,
        };
    }
}
