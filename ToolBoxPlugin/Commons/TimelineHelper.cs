using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Commons
{
    internal static class TimelineHelper
    {
        public static int GetCurrentLayer(Timeline timeline)
        {
            try
            {
                var layers = timeline.LayerSelection.GetSelectedLayers().ToList();
                if (layers.Count > 0)
                    return Math.Max(0, layers.Min());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to read the layer selection: {ex}");
            }

            var selected = timeline.SelectedItems?.FirstOrDefault();
            return selected?.Layer ?? 0;
        }

        public static IReadOnlyList<int> GetSelectedLayers(Timeline timeline)
        {
            var layers = new SortedSet<int>();

            try
            {
                foreach (var layer in timeline.LayerSelection.GetSelectedLayers())
                {
                    if (layer >= 0)
                        layers.Add(layer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to read the layer selection: {ex}");
            }

            return [.. layers];
        }

        public static bool AddItems(Timeline timeline, IReadOnlyList<IItem> items, int frame, int layer)
        {
            if (items.Count == 0)
                return false;

            RemapGroups(timeline, items);
            Reposition(items, frame, Math.Max(0, layer));

            if (timeline.TryAddItems([.. items], frame, Math.Max(0, layer)))
            {
                timeline.ResolveItemCollision(items);
                timeline.RefreshTimelineLengthAndMaxLayer();
                return true;
            }

            var fallbackLayer = timeline.MaxLayer + 1;
            Reposition(items, frame, fallbackLayer);

            if (!timeline.TryAddItems([.. items], frame, fallbackLayer))
                return false;

            timeline.ResolveItemCollision(items);
            timeline.RefreshTimelineLengthAndMaxLayer();
            return true;
        }

        private static void Reposition(IReadOnlyList<IItem> items, int frame, int layer)
        {
            var baseFrame = items.Min(item => item.Frame);
            var baseLayer = items.Min(item => item.Layer);

            foreach (var item in items)
            {
                item.Frame = Math.Max(0, item.Frame - baseFrame + frame);
                item.Layer = Math.Max(0, item.Layer - baseLayer + layer);
            }
        }

        private static void RemapGroups(Timeline timeline, IReadOnlyList<IItem> items)
        {
            if (items.All(item => item.Group == 0))
                return;

            var used = timeline.Items.Select(item => item.Group).ToHashSet();
            var next = used.Count == 0 ? 1 : Math.Max(1, used.Max() + 1);
            var map = new Dictionary<int, int>();

            foreach (var item in items)
            {
                if (item.Group == 0)
                    continue;

                if (!map.TryGetValue(item.Group, out var replacement))
                {
                    replacement = next++;
                    map[item.Group] = replacement;
                }

                item.Group = replacement;
            }
        }
    }
}
