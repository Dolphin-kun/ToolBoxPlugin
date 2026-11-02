using System.Globalization;

namespace ToolBox.ViewModel
{
    public sealed class SettingsSearchResult(object target, string displayName, string iconPath, string groupName, IEnumerable<string> keywords)
    {
        private const CompareOptions MatchOptions =
            CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;

        private readonly string[] keywords = [displayName, groupName, .. keywords];

        public object Target { get; } = target;
        public string DisplayName { get; } = displayName;
        public string IconPath { get; } = iconPath;
        public string GroupName { get; } = groupName;

        public bool Matches(IReadOnlyList<string> terms)
        {
            var compare = CultureInfo.CurrentCulture.CompareInfo;
            return terms.All(term => keywords.Any(keyword => compare.IndexOf(keyword, term, MatchOptions) >= 0));
        }
    }
}
