using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Controls;
using ToolBox.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.ItemEditor.CustomVisibilityAttributes;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Easing.Settings
{
    public enum EasingKind
    {
        [Display(Name = nameof(Texts.EasingKindPreset), ResourceType = typeof(Texts))]
        Preset,
        [Display(Name = nameof(Texts.EasingKindBezier), ResourceType = typeof(Texts))]
        Bezier,
    }

    public enum EasingScope
    {
        [Display(Name = nameof(Texts.EasingScopeAll), ResourceType = typeof(Texts))]
        All,
        [Display(Name = nameof(Texts.EasingScopeDraw), ResourceType = typeof(Texts))]
        Draw,
    }

    public sealed record BezierPointData(float X, float Y, float In1X, float In1Y, float Out2X, float Out2Y);

    public class EasingSettings : ToolItemSettings
    {
        private EasingKind kind = EasingKind.Preset;
        [Display(GroupName = nameof(Texts.EasingDisplayName), Name = nameof(Texts.EasingKindLabel), Order = 0, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public EasingKind Kind { get => kind; set => Set(ref kind, value, etcChangedPropertyNames: [nameof(IsPreset), nameof(IsBezier)]); }

        [JsonIgnore, NJsonIgnore]
        public bool IsPreset => Kind == EasingKind.Preset;

        [JsonIgnore, NJsonIgnore]
        public bool IsBezier => Kind == EasingKind.Bezier;

        private EasingType curve = EasingType.Cubic;
        [Display(GroupName = nameof(Texts.EasingDisplayName), Name = nameof(Texts.EasingCurveLabel), Order = 1, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(IsPreset), true)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EasingType Curve { get => curve; set => Set(ref curve, value); }

        private EasingMode direction = EasingMode.InOut;
        [Display(GroupName = nameof(Texts.EasingDisplayName), Name = nameof(Texts.EasingDirectionLabel), Order = 2, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(IsPreset), true)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EasingMode Direction { get => direction; set => Set(ref direction, value); }

        [JsonIgnore, NJsonIgnore]
        [Display(GroupName = nameof(Texts.EasingDisplayName), Name = nameof(Texts.EasingKindBezier), Order = 3, ResourceType = typeof(Texts))]
        [FixedHeightBezierEditor(220)]
        [ShowPropertyEditorWhen(nameof(IsBezier), true)]
        public BezierAnimation Bezier { get; } = new()
        {
            Points =
            [
                new BezierAnimationPoint(Vector2.Zero, new Vector2(-0.42f, 0), new Vector2(0.42f, 0)),
                new BezierAnimationPoint(Vector2.One, new Vector2(-0.42f, 0), new Vector2(0.42f, 0)),
            ],
        };

        public List<BezierPointData> BezierPoints
        {
            get => [.. Bezier.Points.Select(p => new BezierPointData(p.Point.X, p.Point.Y, p.ControlPoint1.X, p.ControlPoint1.Y, p.ControlPoint2.X, p.ControlPoint2.Y))];
            set
            {
                if (value is not { Count: >= 2 })
                    return;

                Bezier.Points = [.. value.Select(p => new BezierAnimationPoint(new Vector2(p.X, p.Y), new Vector2(p.In1X, p.In1Y), new Vector2(p.Out2X, p.Out2Y)))];
            }
        }

        private bool isSmooth;
        [Display(GroupName = nameof(Texts.EasingDisplayName), Name = nameof(Texts.EasingSmoothLabel), Order = 4, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool IsSmooth { get => isSmooth; set => Set(ref isSmooth, value); }

        private EasingScope scope = EasingScope.All;
        [Display(GroupName = nameof(Texts.EasingDisplayName), Name = nameof(Texts.EasingScopeLabel), Order = 5, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public EasingScope Scope { get => scope; set => Set(ref scope, value); }

        public EasingSettings()
        {
            Bezier.Changed += (_, _) => OnPropertyChanged(nameof(BezierPoints));
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Animation;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Easing";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.ChartBellCurveCumulative;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.EasingDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.EasingDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("ApplyEasingCommand");
    }
}
