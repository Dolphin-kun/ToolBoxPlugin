namespace ToolBox.Items.TextTag.Tags
{
    public static class TextTagBuilder
    {
        public static string BuildOpen(TextTagKind kind, TextTagParameters p, string body) => kind switch
        {
            TextTagKind.Color => "<#" + TextTagParameters.ToHex(p.Color)
                + (p.UseDecorationColor ? "," + TextTagParameters.ToHex(p.DecorationColor) : string.Empty) + ">"
                + DecorationSwitch(p, "+"),

            TextTagKind.Size => "<s" + TextTagParameters.Number(p.FontSize)
                + Trail(p.SelectedFontFamily, Decorated(p.StyleFlags, p.DecorationType)) + ">",

            TextTagKind.Font => "<@" + p.SelectedFontFamily
                + Trail(Decorated(p.StyleFlags, p.DecorationType)) + ">",

            TextTagKind.Position => "<p" + Coordinate(p) + ">",
            TextTagKind.PositionByFont => "<pp" + Coordinate(p) + ">",

            TextTagKind.Rotate => "<rot" + TextTagParameters.Number(p.Angle) + ">",
            TextTagKind.LetterSpacing => "<ls" + TextTagParameters.Number(p.Spacing) + ">",

            TextTagKind.Ruby => BuildRuby(p, body),

            TextTagKind.Speed => "<r" + TextTagParameters.Number(p.Speed) + ">",
            TextTagKind.Wait => "<w" + (p.PerCharacter ? "*" : string.Empty) + TextTagParameters.Number(p.Seconds) + ">",
            TextTagKind.Clear => p.Seconds > 0 ? "<c" + TextTagParameters.Number(p.Seconds) + ">" : "<c>",

            _ => string.Empty,
        };

        public static string BuildClose(TextTagKind kind, TextTagParameters p) => kind switch
        {
            TextTagKind.Color => DecorationSwitch(p, "-") + "<#>",
            TextTagKind.Size => "<s>",
            TextTagKind.Font => "<@>",
            TextTagKind.Position => "<p0,0>",
            TextTagKind.PositionByFont => "<pp0,0>",
            TextTagKind.Rotate => "<rot>",
            TextTagKind.LetterSpacing => "<ls>",
            _ => string.Empty,
        };

        private static string BuildRuby(TextTagParameters p, string body)
        {
            var parts = new List<string>
            {
                Escape(body),
                Escape(p.RubyText),
                p.RubyScale == TextTagParameters.DefaultRubyScale
                    ? string.Empty
                    : TextTagParameters.Number(p.RubyScale),
                p.X == 0 ? string.Empty : TextTagParameters.Number(p.X),
                p.Y == 0 ? string.Empty : TextTagParameters.Number(p.Y),
                Escape(p.SelectedFontFamily),
                p.RubyStyleFlags,
                p.UseColor ? TextTagParameters.ToHex(p.Color) : string.Empty,
                p.UseDecorationColor ? TextTagParameters.ToHex(p.DecorationColor) : string.Empty,
            };

            while (parts.Count > 2 && string.IsNullOrEmpty(parts[^1]))
                parts.RemoveAt(parts.Count - 1);

            return "<rb" + string.Join(",", parts) + ">";
        }

        private static string Coordinate(TextTagParameters p) => p.IsRelative
            ? TextTagParameters.Signed(p.X) + "," + TextTagParameters.Signed(p.Y)
            : TextTagParameters.Number(p.X) + "," + TextTagParameters.Number(p.Y);

        private static string Decorated(string flags, int decorationType) =>
            decorationType >= 0 ? flags + decorationType : flags;

        private static string DecorationSwitch(TextTagParameters p, string sign) =>
            p.DecorationType >= 0 ? "<@" + sign + p.DecorationType + ">" : string.Empty;

        private static string Trail(params string[] values)
        {
            var parts = values.ToList();

            while (parts.Count > 0 && string.IsNullOrEmpty(parts[^1]))
                parts.RemoveAt(parts.Count - 1);

            return parts.Count == 0 ? string.Empty : "," + string.Join(",", parts);
        }

        public static string Escape(string value) =>
            string.IsNullOrEmpty(value) ? string.Empty : value.Replace(",", "\\,");
    }
}
