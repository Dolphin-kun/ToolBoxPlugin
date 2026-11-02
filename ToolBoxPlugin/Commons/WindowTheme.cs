using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using YukkuriMovieMaker.Plugin;

namespace ToolBox.Commons
{
    public static partial class WindowTheme
    {
        private const int DwmaBorderColor = 34;
        private const int DwmaCaptionColor = 35;
        private const int DwmaTextColor = 36;
        private const uint DwmColorDefault = 0xFFFFFFFF;

        public static void Attach(Window window)
        {
            window.SetResourceReference(Window.BackgroundProperty, SystemColors.ControlBrushKey);
            window.SetResourceReference(Window.ForegroundProperty, SystemColors.WindowTextBrushKey);

            void OnThemeChanged(object? sender, ThemeManager.ThemeChangedEventArgs e) => ApplyTitleBar(window);

            window.SourceInitialized += (_, _) => ApplyTitleBar(window);
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
            window.Closed += (_, _) => ThemeManager.Instance.ThemeChanged -= OnThemeChanged;
        }

        private static void ApplyTitleBar(Window window)
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
                return;

            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
                return;

            if (ThemeManager.Instance.CurrentThemeName is "Windows" or "NormalColor")
            {
                SetColor(handle, DwmaCaptionColor, DwmColorDefault);
                SetColor(handle, DwmaBorderColor, DwmColorDefault);
                SetColor(handle, DwmaTextColor, DwmColorDefault);
                return;
            }

            var caption = ToColorRef((window.TryFindResource(SystemColors.ControlBrushKey) as SolidColorBrush)?.Color ?? Colors.White);
            var text = ToColorRef((window.TryFindResource(SystemColors.WindowTextBrushKey) as SolidColorBrush)?.Color ?? Colors.White);
            SetColor(handle, DwmaCaptionColor, caption);
            SetColor(handle, DwmaBorderColor, caption);
            SetColor(handle, DwmaTextColor, text);
        }

        private static void SetColor(IntPtr handle, int attribute, uint color) => _ = DwmSetWindowAttribute(handle, attribute, ref color, sizeof(uint));

        private static uint ToColorRef(Color color) => (uint)(color.R | (color.G << 8) | (color.B << 16));

        [LibraryImport("dwmapi.dll")]
        private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref uint value, int size);
    }
}
