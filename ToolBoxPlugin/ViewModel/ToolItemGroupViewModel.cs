using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ToolItemGroupViewModel(string categoryName, IReadOnlyList<ToolItemEntryViewModel> items) : Bindable
    {
        public string CategoryName { get; } = categoryName;
        public IReadOnlyList<ToolItemEntryViewModel> Items { get; } = items;
        public bool HasChildren => Items.Count > 0;

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }
    }
}
