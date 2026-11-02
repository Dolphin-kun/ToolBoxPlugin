using System.Reflection;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Commons
{
    public static class AnimationHelper
    {
        private static readonly MethodInfo? getAnimatables =
            typeof(Animatable).GetMethod("GetAnimatables", BindingFlags.Instance | BindingFlags.NonPublic);

        public static IEnumerable<Animation> GetAnimations(IItem item)
        {
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var result = new List<Animation>();
            Collect(item, visited, result);
            return result;
        }

        public static IEnumerable<Animation> GetDrawAnimations(IItem item) =>
            item is IVideoItem video ? [video.X, video.Y, video.Z, video.Opacity, video.Zoom, video.Rotation] : [];

        private static void Collect(object owner, HashSet<object> visited, List<Animation> result)
        {
            if (!visited.Add(owner))
                return;

            if (owner is Animation animation)
            {
                result.Add(animation);
                return;
            }

            foreach (var child in GetChildren(owner))
                Collect(child, visited, result);
        }

        public static IEnumerable<IAnimatable> GetChildren(object owner) =>
            owner is Animatable animatable && getAnimatables?.Invoke(animatable, null) is IEnumerable<IAnimatable> children
                ? children.Where(child => child != null)
                : [];

        public static bool IsMoving(Animation animation) => animation.AnimationType switch
        {
            AnimationType.なし or AnimationType.移動量指定 or AnimationType.ランダム移動 or AnimationType.反復移動 => false,
            _ => true,
        };

        public static void Reverse(Animation animation, int length)
        {
            switch (animation.AnimationType)
            {
                case AnimationType.なし:
                    return;

                case AnimationType.移動量指定:
                    var values = animation.Values;
                    if (values.Count < 2)
                        return;

                    var start = values[0].Value;
                    var delta = values[1].Value;
                    values[0].Value = start + delta * length;
                    values[1].Value = -delta;
                    return;
            }

            var active = animation.ActiveValues.ToList();
            for (int i = 0, j = active.Count - 1; i < j; i++, j--)
                (active[i].Value, active[j].Value) = (active[j].Value, active[i].Value);

            if (animation.AnimationType is AnimationType.Linear_Bezier or AnimationType.Interpolation_Bezier)
                animation.Bezier.Reverse();
            else
                animation.AnimationType = SwapEasingDirection(animation.AnimationType);
        }

        private static AnimationType SwapEasingDirection(AnimationType type)
        {
            var parts = type.ToString().Split('_');
            if (parts.Length < 2)
                return type;

            parts[1] = parts[1] switch
            {
                "In" => "Out",
                "Out" => "In",
                _ => parts[1],
            };

            return Enum.TryParse<AnimationType>(string.Join('_', parts), out var swapped) ? swapped : type;
        }

        public static void ReverseKeyFrames(KeyFrames keyFrames, int length)
        {
            var frames = keyFrames.Frames;
            for (var i = 0; i < frames.Count; i++)
                keyFrames.SetFrame(i, length - frames[frames.Count - 1 - i]);
        }

        public static void ShiftAt(Animation animation, IItem item, int frame, double current, double delta)
        {
            var count = animation.ActiveValues.Count();
            if (count <= 1 || !animation.AnimationType.IsKeyFrameSupported())
            {
                animation.AddToEachValues(delta);
                return;
            }

            var index = frame <= 0 ? 0
                : frame >= item.Length - 1 ? count - 1
                : Math.Min(count - 1, item.KeyFrames.Frames.Count(keyFrame => keyFrame < frame) + 1);

            animation.Values[index].Value = current + delta;
        }

        public static bool MirrorKeyFrames(IItem item)
        {
            var keyFrames = item.KeyFrames;
            var length = item.Length;
            var original = keyFrames.Frames.ToList();
            var animations = GetAnimations(item).ToList();
            var snapshots = animations.Select(animation => animation.Values.Select(value => value.Value).ToList()).ToList();

            var changed = false;
            for (var i = 0; i < original.Count; i++)
            {
                var target = length - original[i];
                if (target <= 0 || target >= length || keyFrames.Frames.Contains(target))
                    continue;

                var counts = animations.Select(animation => animation.Values.Count).ToList();
                var index = keyFrames.Insert(target);

                for (var j = 0; j < animations.Count; j++)
                {
                    var values = animations[j].Values;
                    if (values.Count > counts[j] && index + 1 < values.Count && i + 1 < snapshots[j].Count)
                        values[index + 1].Value = snapshots[j][i + 1];
                }

                changed = true;
            }

            return changed;
        }

        public static bool ShiftKeyFrames(KeyFrames keyFrames, int length, int delta)
        {
            var frames = keyFrames.Frames;
            if (frames.Count == 0 || delta == 0)
                return false;

            delta = Math.Clamp(delta, 1 - frames[0], length - 1 - frames[^1]);
            if (delta == 0)
                return false;

            for (var i = 0; i < frames.Count; i++)
                keyFrames.SetFrame(i, frames[i] + delta);

            return true;
        }
    }
}
