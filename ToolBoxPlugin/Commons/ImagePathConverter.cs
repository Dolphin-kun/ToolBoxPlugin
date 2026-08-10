using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ToolBox.Commons
{
    /// <summary>
    /// 画像のパスを <see cref="System.Windows.Controls.Image"/> に渡すための変換。
    /// パスが未設定だったり見つからなかったりしたときは null を返す。
    /// 変換せずに直接束縛すると、空文字のときに WPF 既定の変換が例外を出してしまう。
    /// </summary>
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
                // 読み込んだ後もファイルを掴んだままにしないための指定
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
