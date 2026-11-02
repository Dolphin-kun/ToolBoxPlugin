namespace ToolBox.ViewModel
{
    public class ToolItemGroupViewModel(string categoryName, IReadOnlyList<ToolItemEntryViewModel> items)
        : SettingsNodeViewModel(categoryName)
    {
        public IReadOnlyList<ToolItemEntryViewModel> Items { get; } = items;
        public override bool HasChildren => Items.Count > 0;
    }
}
