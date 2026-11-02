using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public sealed class LauncherCandidate(string displayName, string groupName, string iconPath, Func<LauncherNode> create) : Bindable
    {
        public string DisplayName { get; } = displayName;
        public string GroupName { get; } = groupName;
        public string IconPath { get; } = iconPath;

        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set => Set(ref isChecked, value);
        }

        public LauncherNode CreateNode() => create();
    }
}
