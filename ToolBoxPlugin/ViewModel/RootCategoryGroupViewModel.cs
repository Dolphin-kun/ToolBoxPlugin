using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class RootCategoryGroupViewModel(string categoryName, IReadOnlyList<object> subGroups) : Bindable
    {
        public string CategoryName { get; } = categoryName;
        public IReadOnlyList<object> SubGroups { get; } = subGroups;
        public bool HasChildren => SubGroups.Count > 0;

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }
    }
}
