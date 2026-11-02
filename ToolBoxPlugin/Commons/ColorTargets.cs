using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Commons
{
    public sealed record ColorTarget(string Label, object Owner, PropertyInfo Property)
    {
        public Color Value
        {
            get => Property.GetValue(Owner) is Color color ? color : Colors.Transparent;
            set => Property.SetValue(Owner, value);
        }
    }

    public static class ColorTargets
    {
        private const string ItemColorPropertyName = "ItemColor";

        public static IReadOnlyList<ColorTarget> Find(IItem item)
        {
            var result = new List<ColorTarget>();
            Collect(item, null, new HashSet<object>(ReferenceEqualityComparer.Instance), result);
            return result;
        }

        public static IReadOnlyList<ColorTarget> Find(IEnumerable<IItem> items) => [.. items.SelectMany(Find)];

        private static void Collect(object owner, string? prefix, HashSet<object> visited, List<ColorTarget> result)
        {
            if (owner is Animation || !visited.Add(owner))
                return;

            var ownerPrefix = owner is IVideoEffect effect ? effect.Label : prefix;

            foreach (var property in owner.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.PropertyType != typeof(Color)
                    || !property.CanWrite
                    || property.Name == ItemColorPropertyName
                    || property.GetIndexParameters().Length > 0
                    || property.GetCustomAttribute<ColorPickerAttribute>() == null)
                    continue;

                var name = property.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? property.Name;
                result.Add(new ColorTarget(ownerPrefix is null ? name : $"{ownerPrefix} / {name}", owner, property));
            }

            foreach (var child in AnimationHelper.GetChildren(owner))
                Collect(child, ownerPrefix, visited, result);
        }
    }
}
