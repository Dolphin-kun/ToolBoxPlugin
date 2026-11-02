using System.Reflection;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Commons
{
    public enum HorizontalAnchor
    {
        Left,
        Center,
        Right,
    }

    public enum VerticalAnchor
    {
        Top,
        Center,
        Bottom,
    }

    public static class CenterPointEffectHelper
    {
        private const string EffectTypeName = "YukkuriMovieMaker.Project.Effects.CenterPointEffect, YukkuriMovieMaker";

        private static readonly Type? effectType = Type.GetType(EffectTypeName);
        private static readonly PropertyInfo? horizontalProperty = effectType?.GetProperty("Horizontal");
        private static readonly PropertyInfo? verticalProperty = effectType?.GetProperty("Vertical");
        private static readonly PropertyInfo? keepPositionProperty = effectType?.GetProperty("IsKeepPosition");

        public static bool IsAvailable =>
            effectType != null && horizontalProperty != null && verticalProperty != null && keepPositionProperty != null;

        public static void Remove(IVideoItem item)
        {
            if (effectType != null && item.VideoEffects.Any(e => e.GetType() == effectType))
                item.VideoEffects = item.VideoEffects.RemoveAll(e => e.GetType() == effectType);
        }

        public static void Apply(IVideoItem item, HorizontalAnchor? horizontal, VerticalAnchor? vertical, bool keepPosition)
        {
            if (!IsAvailable)
                return;

            var effect = item.VideoEffects.FirstOrDefault(e => e.GetType() == effectType);
            if (effect == null)
            {
                if (Activator.CreateInstance(effectType!) is not IVideoEffect created)
                    return;

                effect = created;
                item.VideoEffects = item.VideoEffects.Add(effect);
            }

            if (horizontal is { } h)
                horizontalProperty!.SetValue(effect, Enum.Parse(horizontalProperty.PropertyType, h.ToString()));

            if (vertical is { } v)
                verticalProperty!.SetValue(effect, Enum.Parse(verticalProperty.PropertyType, v.ToString()));

            keepPositionProperty!.SetValue(effect, keepPosition);
        }
    }
}
