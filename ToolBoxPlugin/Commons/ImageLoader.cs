using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ToolBox.Commons
{
    public sealed record AnimationFrame(BitmapSource Image, TimeSpan Delay);

    public static class ImageLoader
    {
        private const long FrameBudgetBytes = 32L * 1024 * 1024;

        private const int MaxDimension = 384;

        private const int MaxFrameCount = 600;

        private static readonly TimeSpan MinDelay = TimeSpan.FromMilliseconds(20);
        private static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(100);

        private static readonly Dictionary<string, IReadOnlyList<AnimationFrame>> cache = [];

        public static BitmapSource? LoadStatic(string? path)
        {
            if (!TryGetUri(path, out var uri))
                return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = uri;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        public static IReadOnlyList<AnimationFrame>? LoadAnimation(string? path)
        {
            if (!TryGetUri(path, out var uri))
                return null;

            var key = BuildCacheKey(uri);
            if (cache.TryGetValue(key, out var cached))
                return cached.Count > 0 ? cached : null;

            IReadOnlyList<AnimationFrame> frames;
            try
            {
                frames = Decode(uri);
            }
            catch
            {
                frames = [];
            }

            cache[key] = frames;
            return frames.Count > 0 ? frames : null;
        }

        private static IReadOnlyList<AnimationFrame> Decode(Uri uri)
        {
            var decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            if (decoder is not GifBitmapDecoder || decoder.Frames.Count < 2)
                return [];

            if (decoder.Frames.Count > MaxFrameCount)
                return [];

            var width = GetUShort(decoder.Metadata, "/logscrdesc/Width") ?? decoder.Frames[0].PixelWidth;
            var height = GetUShort(decoder.Metadata, "/logscrdesc/Height") ?? decoder.Frames[0].PixelHeight;
            if (width <= 0 || height <= 0)
                return [];

            var scale = CalculateScale(width, height, decoder.Frames.Count);
            var scaledWidth = Math.Max(1, (int)Math.Round(width * scale));
            var scaledHeight = Math.Max(1, (int)Math.Round(height * scale));

            var result = new List<AnimationFrame>(decoder.Frames.Count);
            BitmapSource? carried = null;

            foreach (var frame in decoder.Frames)
            {
                var meta = frame.Metadata as BitmapMetadata;
                var left = GetUShort(meta, "/imgdesc/Left") ?? 0;
                var top = GetUShort(meta, "/imgdesc/Top") ?? 0;
                var disposal = GetByte(meta, "/grctlext/Disposal") ?? 0;
                var delayHundredths = GetUShort(meta, "/grctlext/Delay") ?? 0;

                var area = new Rect(left * scale, top * scale, frame.PixelWidth * scale, frame.PixelHeight * scale);
                var composed = Compose(carried, frame, area, scaledWidth, scaledHeight);

                var delay = delayHundredths > 0
                    ? TimeSpan.FromMilliseconds(delayHundredths * 10)
                    : DefaultDelay;
                result.Add(new AnimationFrame(composed, delay < MinDelay ? MinDelay : delay));

                carried = disposal switch
                {
                    2 => Erase(composed, area, scaledWidth, scaledHeight),
                    3 => carried,
                    _ => composed,
                };
            }

            return result;
        }

        private static double CalculateScale(int width, int height, int frameCount)
        {
            var scale = 1.0;

            var longest = Math.Max(width, height);
            if (longest > MaxDimension)
                scale = (double)MaxDimension / longest;

            var bytes = (long)(width * scale) * (long)(height * scale) * 4 * frameCount;
            if (bytes > FrameBudgetBytes)
                scale *= Math.Sqrt((double)FrameBudgetBytes / bytes);

            return scale;
        }

        private static BitmapSource Compose(BitmapSource? background, BitmapSource frame, Rect area, int width, int height)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                if (background != null)
                    dc.DrawImage(background, new Rect(0, 0, width, height));

                dc.DrawImage(frame, area);
            }

            return Render(visual, width, height);
        }

        private static BitmapSource Erase(BitmapSource source, Rect area, int width, int height)
        {
            var full = new RectangleGeometry(new Rect(0, 0, width, height));
            var hole = new RectangleGeometry(area);

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.PushClip(new CombinedGeometry(GeometryCombineMode.Exclude, full, hole));
                dc.DrawImage(source, new Rect(0, 0, width, height));
                dc.Pop();
            }

            return Render(visual, width, height);
        }

        private static BitmapSource Render(DrawingVisual visual, int width, int height)
        {
            var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);
            target.Freeze();
            return target;
        }

        private static bool TryGetUri(string? path, out Uri uri)
        {
            uri = null!;

            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (!Uri.TryCreate(path, UriKind.Absolute, out var parsed))
                return false;

            if (parsed.IsFile && !File.Exists(parsed.LocalPath))
                return false;

            uri = parsed;
            return true;
        }

        private static string BuildCacheKey(Uri uri)
        {
            if (!uri.IsFile)
                return uri.AbsoluteUri;

            try
            {
                return uri.LocalPath + "|" + File.GetLastWriteTimeUtc(uri.LocalPath).Ticks;
            }
            catch
            {
                return uri.LocalPath;
            }
        }

        private static ushort? GetUShort(BitmapMetadata? metadata, string query) =>
            Query(metadata, query) as ushort?;

        private static byte? GetByte(BitmapMetadata? metadata, string query) =>
            Query(metadata, query) as byte?;

        private static object? Query(BitmapMetadata? metadata, string query)
        {
            try
            {
                return metadata?.GetQuery(query);
            }
            catch
            {
                return null;
            }
        }
    }
}
