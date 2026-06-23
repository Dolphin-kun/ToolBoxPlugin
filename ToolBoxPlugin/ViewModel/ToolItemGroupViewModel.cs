using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class ToolItemGroupViewModel(string categoryName, IReadOnlyList<ToolItemEntryViewModel> items) : Bindable
    {
        public string CategoryName { get; } = categoryName;
        public IReadOnlyList<ToolItemEntryViewModel> Items { get; } = items;

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }
    }
}
