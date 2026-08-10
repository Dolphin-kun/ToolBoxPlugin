using ToolBox.Items.Shortcuts;
using ToolBox.Settings;

namespace ToolBox.ViewModel
{
    public class ShortcutsGroupViewModel : SettingsNodeViewModel
    {
        public IReadOnlyList<ShortcutEntryViewModel> Shortcuts { get; }

        public override bool HasChildren => Shortcuts.Count > 0;

        public ShortcutsGroupViewModel(ToolBoxPanel panel, string group) : base(group)
        {
            Shortcuts = [.. ShortcutDefinitions.InGroup(group).Select(def => new ShortcutEntryViewModel(panel, def))];
            IsExpanded = false;
        }
    }
}
