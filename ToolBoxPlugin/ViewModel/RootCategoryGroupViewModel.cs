using System.Collections.Generic;
using ToolBox.Commons;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class RootCategoryGroupViewModel(string categoryName, IReadOnlyList<object> subGroups) : Bindable
    {
        public string CategoryName { get; } = categoryName;
        public IReadOnlyList<object> SubGroups { get; } = subGroups;

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }
    }
}
