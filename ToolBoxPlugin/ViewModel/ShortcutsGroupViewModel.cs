using ToolBox.Items.Shortcuts;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ShortcutsGroupViewModel(ToolBoxPanel panel) : Bindable
    {
        public string CategoryName { get; } = "ショートカット";

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }

        public IReadOnlyList<ShortcutEntryViewModel> Shortcuts { get; } = [.. ShortcutDefinitions.Allowed.Select(def => new ShortcutEntryViewModel(panel, def))];

        public bool HasChildren => Shortcuts.Count > 0;
    }
}
