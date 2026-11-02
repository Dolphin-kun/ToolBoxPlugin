using System.Globalization;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Items.TextTag.Tags
{
    public class TextTagParameters : Bindable
    {
        public const double DefaultRubyScale = 50;

        private Color color = Colors.Red;
        public Color Color { get => color; set => Set(ref color, value); }

        private bool useColor;
        public bool UseColor { get => useColor; set => Set(ref useColor, value); }

        private bool useDecorationColor;
        public bool UseDecorationColor { get => useDecorationColor; set => Set(ref useDecorationColor, value); }

        private Color decorationColor = Colors.Black;
        public Color DecorationColor { get => decorationColor; set => Set(ref decorationColor, value); }

        private double fontSize = 60;
        public double FontSize { get => fontSize; set => Set(ref fontSize, Math.Clamp(value, 1, 999)); }

        public const string DefaultFontFamily = "メイリオ";

        private string fontFamily = DefaultFontFamily;
        public string FontFamily { get => fontFamily; set => Set(ref fontFamily, value ?? string.Empty); }

        private bool useFontFamily;
        public bool UseFontFamily { get => useFontFamily; set => Set(ref useFontFamily, value); }

        public string SelectedFontFamily => UseFontFamily ? FontFamily : string.Empty;

        private bool bold;
        public bool Bold { get => bold; set => Set(ref bold, value); }

        private bool italic;
        public bool Italic { get => italic; set => Set(ref italic, value); }

        private bool strike;
        public bool Strike { get => strike; set => Set(ref strike, value); }

        private bool underline;
        public bool Underline { get => underline; set => Set(ref underline, value); }

        private int decorationType = -1;
        public int DecorationType { get => decorationType; set => Set(ref decorationType, Math.Clamp(value, -1, 6)); }

        private double x;
        public double X { get => x; set => Set(ref x, value); }

        private double y;
        public double Y { get => y; set => Set(ref y, value); }

        private bool isRelative = true;
        public bool IsRelative { get => isRelative; set => Set(ref isRelative, value); }

        private double angle = 30;
        public double Angle { get => angle; set => Set(ref angle, value); }

        private double spacing = 10;
        public double Spacing { get => spacing; set => Set(ref spacing, value); }

        private string rubyText = string.Empty;
        public string RubyText { get => rubyText; set => Set(ref rubyText, value ?? string.Empty); }

        private double rubyScale = DefaultRubyScale;
        public double RubyScale { get => rubyScale; set => Set(ref rubyScale, Math.Clamp(value, 1, 400)); }

        private double speed = 20;
        public double Speed { get => speed; set => Set(ref speed, Math.Max(0, value)); }

        private double seconds = 1;
        public double Seconds { get => seconds; set => Set(ref seconds, Math.Max(0, value)); }

        private bool perCharacter;
        public bool PerCharacter { get => perCharacter; set => Set(ref perCharacter, value); }

        public string StyleFlags
        {
            get
            {
                var flags = string.Empty;
                if (Bold) flags += "B";
                if (Italic) flags += "I";
                if (Strike) flags += "S";
                if (Underline) flags += "U";
                return flags;
            }
        }

        public string RubyStyleFlags
        {
            get
            {
                var flags = string.Empty;
                if (Bold) flags += "B";
                if (Italic) flags += "I";
                return flags;
            }
        }

        public static string ToHex(Color value) =>
            value.A == 0xFF
                ? $"{value.R:x2}{value.G:x2}{value.B:x2}"
                : $"{value.R:x2}{value.G:x2}{value.B:x2}{value.A:x2}";

        public static string Number(double value) =>
            value.ToString(value == Math.Floor(value) ? "0" : "0.##", CultureInfo.InvariantCulture);

        public static string Signed(double value) =>
            (value < 0 ? string.Empty : "+") + Number(value);
    }
}
