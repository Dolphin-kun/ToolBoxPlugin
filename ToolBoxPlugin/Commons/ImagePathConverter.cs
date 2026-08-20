using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ToolBox.Commons
{
    public sealed class ImagePathConverter : IValueConverter
    {
        public static ImagePathConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
                return null;

            if (!Uri.TryCreate(path, UriKind.Absolute, out var uri))
                return null;

            if (uri.IsFile && !File.Exists(uri.LocalPath))
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

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            Binding.DoNothing;
    }
}
