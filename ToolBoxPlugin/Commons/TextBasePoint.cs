using System.Reflection;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Commons
{
    public static class TextBasePoint
    {
        private const string PropertyName = "BasePoint";
        private const string VerticalPrefix = "V";

        private static PropertyInfo? FindProperty(IItem item) =>
            item.GetType().GetProperty(PropertyName) is { CanWrite: true } property && property.PropertyType == typeof(BasePoint)
                ? property
                : null;

        public static bool CanSet(IItem item) => FindProperty(item) != null;

        public static bool TrySet(IItem item, PlacementPosition position)
        {
            if (FindProperty(item) is not { } property || property.GetValue(item) is not BasePoint current)
                return false;

            var name = current.ToString().StartsWith(VerticalPrefix, StringComparison.Ordinal)
                ? $"{VerticalPrefix}{position.Vertical}{position.Horizontal}"
                : $"{position.Horizontal}{position.Vertical}";

            if (!Enum.TryParse<BasePoint>(name, out var basePoint))
                return false;

            property.SetValue(item, basePoint);
            return true;
        }
    }
}
