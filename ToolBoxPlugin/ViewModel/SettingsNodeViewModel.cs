using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public abstract class SettingsNodeViewModel(string categoryName) : Bindable
    {
        public string CategoryName { get; } = categoryName;

        public virtual bool HasChildren => false;

        private bool isExpanded = true;
        public bool IsExpanded
        {
            get => isExpanded;
            set => Set(ref isExpanded, value);
        }
    }
}
