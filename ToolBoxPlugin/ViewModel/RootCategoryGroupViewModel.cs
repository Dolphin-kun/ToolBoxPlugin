namespace ToolBox.ViewModel
{
    public class RootCategoryGroupViewModel(string categoryName, IReadOnlyList<object> subGroups)
        : SettingsNodeViewModel(categoryName)
    {
        public IReadOnlyList<object> SubGroups { get; } = subGroups;
        public override bool HasChildren => SubGroups.Count > 0;
    }
}
