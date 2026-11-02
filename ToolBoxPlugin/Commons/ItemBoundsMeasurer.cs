using System.Numerics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.ItemEditor;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Commons
{
    public readonly record struct ItemBounds(Vector2 Min, Vector2 Max);

    public sealed class ItemBoundsMeasurer : IDisposable
    {
        private readonly Timeline timeline;
        private readonly Scenes scenes;
        private readonly Scene scene;
        private readonly TimelineSourceAndDevices devices;

        private ItemBoundsMeasurer(Timeline timeline, Scenes scenes, Scene scene)
        {
            this.timeline = timeline;
            this.scenes = scenes;
            this.scene = scene;
            devices = new TimelineSourceAndDevices(scene);
        }

        public static ItemBoundsMeasurer? Create(Timeline timeline, Scenes scenes)
        {
            if (scenes.AllScenes.FirstOrDefault(s => s.Timeline == timeline) is not { } scene)
                return null;

            try
            {
                return new ItemBoundsMeasurer(timeline, scenes, scene);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to prepare the graphics devices: {ex}");
                return null;
            }
        }

        public ItemBounds? Measure(IVideoItem item)
        {
            try
            {
                var info = timeline.VideoInfo;
                var frame = Math.Clamp(timeline.CurrentFrame - item.Frame, 0, Math.Max(0, item.Length - 1));
                var description = new TimelineItemSourceDescription(
                    new TimelineSourceDescription(
                        new System.Drawing.Size(info.Width, info.Height),
                        new YukkuriMovieMaker.Player.Video.FrameTime(timeline.CurrentFrame, info.FPS),
                        new YukkuriMovieMaker.Player.Video.FrameTime(timeline.Length, info.FPS),
                        info.FPS,
                        TimelineSourceUsage.Playing,
                        timeline.ID,
                        scenes.AllScenes.Cast<ISceneInfo>()),
                    frame, item.Length, item.Layer);

                using var source = item.CreateVideoSource(devices.Devices, scene);
                source.Update(description);

                var zoom = (float)(item.Zoom.GetValue(frame, item.Length, info.FPS) / 100);
                var transform = Matrix3x2.CreateScale(zoom)
                    * Matrix3x2.CreateRotation(MathF.PI * (float)item.Rotation.GetValue(frame, item.Length, info.FPS) / 180f);

                ItemBounds? result = null;
                foreach (var output in source.Outputs)
                {
                    if (output.Output is not { } image)
                        continue;

                    var bounds = devices.Devices.DeviceContext.GetImageLocalBounds(image);
                    foreach (var corner in Corners(bounds))
                    {
                        var point = Vector2.Transform(corner, transform) + output.DrawingOffset;
                        result = result is { } current
                            ? new ItemBounds(Vector2.Min(current.Min, point), Vector2.Max(current.Max, point))
                            : new ItemBounds(point, point);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to measure the item: {ex}");
                return null;
            }
        }

        private static IEnumerable<Vector2> Corners(Vortice.RawRectF bounds) =>
        [
            new(bounds.Left, bounds.Top),
            new(bounds.Right, bounds.Top),
            new(bounds.Right, bounds.Bottom),
            new(bounds.Left, bounds.Bottom),
        ];

        public void Dispose() => ((IDisposable)devices).Dispose();
    }
}
