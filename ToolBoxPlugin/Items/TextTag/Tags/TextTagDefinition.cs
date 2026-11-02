namespace ToolBox.Items.TextTag.Tags
{
    public sealed class TextTagDefinition
    {
        public required TextTagKind Kind { get; init; }
        public required TextTagApply Apply { get; init; }
        public required string Label { get; init; }
        public required string Sample { get; init; }
        public required string IconPath { get; init; }
        public IReadOnlyList<TextTagField> Fields { get; init; } = [];

        public bool NeedsSelection => Apply is TextTagApply.Wrap or TextTagApply.Replace;

        public static IReadOnlyList<TextTagDefinition> All =>
        [
            new()
            {
                Kind = TextTagKind.Color,
                Apply = TextTagApply.Wrap,
                Label = Texts.TextTagColorLabel,
                Sample = "<#ff0000>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.Palette,
                Fields = [TextTagField.Color, TextTagField.DecorationColor, TextTagField.DecorationType],
            },
            new()
            {
                Kind = TextTagKind.Size,
                Apply = TextTagApply.Wrap,
                Label = Texts.TextTagSizeLabel,
                Sample = "<s60>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.FormatSize,
                Fields = [TextTagField.FontSize, TextTagField.OptionalFontFamily, TextTagField.Style, TextTagField.DecorationType],
            },
            new()
            {
                Kind = TextTagKind.Font,
                Apply = TextTagApply.Wrap,
                Label = Texts.TextTagFontLabel,
                Sample = "<@メイリオ,B>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.FormatFont,
                Fields = [TextTagField.OptionalFontFamily, TextTagField.Style, TextTagField.DecorationType],
            },
            new()
            {
                Kind = TextTagKind.Position,
                Apply = TextTagApply.Wrap,
                Label = Texts.TextTagPositionLabel,
                Sample = "<p+20,+0>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.ArrowAll,
                Fields = [TextTagField.X, TextTagField.Y, TextTagField.Relative],
            },
            new()
            {
                Kind = TextTagKind.PositionByFont,
                Apply = TextTagApply.Wrap,
                Label = Texts.TextTagPositionByFontLabel,
                Sample = "<pp+100,+0>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.ArrowExpandAll,
                Fields = [TextTagField.X, TextTagField.Y, TextTagField.Relative],
            },
            new()
            {
                Kind = TextTagKind.Rotate,
                Apply = TextTagApply.Wrap,
                Label = Texts.TextTagRotateLabel,
                Sample = "<rot30>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.RotateRight,
                Fields = [TextTagField.Angle],
            },
            new()
            {
                Kind = TextTagKind.LetterSpacing,
                Apply = TextTagApply.Wrap,
                Label = Texts.TextTagLetterSpacingLabel,
                Sample = "<ls10>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.FormatLetterSpacing,
                Fields = [TextTagField.Spacing],
            },
            new()
            {
                Kind = TextTagKind.Ruby,
                Apply = TextTagApply.Replace,
                Label = Texts.TextTagRubyLabel,
                Sample = "<rb東京,とうきょう>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.FuriganaHorizontal,
                Fields =
                [
                    TextTagField.RubyText, TextTagField.RubyScale, TextTagField.X, TextTagField.Y,
                    TextTagField.OptionalFontFamily, TextTagField.Style, TextTagField.OptionalColor, TextTagField.DecorationColor,
                ],
            },
            new()
            {
                Kind = TextTagKind.Speed,
                Apply = TextTagApply.Insert,
                Label = Texts.TextTagSpeedLabel,
                Sample = "<r20>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.Speedometer,
                Fields = [TextTagField.Speed],
            },
            new()
            {
                Kind = TextTagKind.Wait,
                Apply = TextTagApply.Insert,
                Label = Texts.TextTagWaitLabel,
                Sample = "<w1>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.TimerSand,
                Fields = [TextTagField.Seconds, TextTagField.PerCharacter],
            },
            new()
            {
                Kind = TextTagKind.Clear,
                Apply = TextTagApply.Insert,
                Label = Texts.TextTagClearLabel,
                Sample = "<c1>",
                IconPath = YukkuriMovieMaker.Resources.Icons.IconKeys.Eraser,
                Fields = [TextTagField.Seconds],
            },
        ];
    }
}
