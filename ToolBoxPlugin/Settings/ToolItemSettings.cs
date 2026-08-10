using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;

namespace ToolBox.Settings
{
    [JsonConverter(typeof(ToolItemSettingsConverter))]
    public class ToolItemSettings : Bindable
    {
        private Point position;
        public Point Position { get => position; set => Set(ref position, value); }

        private Size size = new(1, 1);
        public Size Size { get => size; set => Set(ref size, value); }

        private ToolItemDisplayMode displayMode = ToolItemDisplayMode.Icon;
        public ToolItemDisplayMode DisplayMode
        {
            get => displayMode;
            set => Set(ref displayMode, value);
        }

        private bool isLocked;
        public bool IsLocked
        {
            get => isLocked;
            set => Set(ref isLocked, value);
        }

        private string customImagePath = string.Empty;
        public string CustomImagePath
        {
            get => customImagePath;
            set => Set(ref customImagePath, value);
        }

        private string label = string.Empty;
        public string Label
        {
            get => string.IsNullOrEmpty(label) ? DisplayName : label;
            set => Set(ref label, value, etcChangedPropertyNames: nameof(Name));
        }

        private string hotkeyGesture = string.Empty;
        public string HotkeyGesture
        {
            get => hotkeyGesture;
            set => Set(ref hotkeyGesture, value);
        }

        #region 配色

        private bool useCustomAppearance;
        public bool UseCustomAppearance
        {
            get => useCustomAppearance;
            set => Set(ref useCustomAppearance, value, etcChangedPropertyNames: AppearancePropertyNames);
        }

        private string backgroundColorHex = DefaultBackgroundColorHex;
        public string BackgroundColorHex
        {
            get => backgroundColorHex;
            set => Set(ref backgroundColorHex, value, etcChangedPropertyNames: [nameof(BackgroundColor), .. AppearancePropertyNames]);
        }

        private string borderColorHex = DefaultBorderColorHex;
        public string BorderColorHex
        {
            get => borderColorHex;
            set => Set(ref borderColorHex, value, etcChangedPropertyNames: [nameof(BorderColor), .. AppearancePropertyNames]);
        }

        private string foregroundColorHex = DefaultForegroundColorHex;
        public string ForegroundColorHex
        {
            get => foregroundColorHex;
            set => Set(ref foregroundColorHex, value, etcChangedPropertyNames: [nameof(ForegroundColor), .. AppearancePropertyNames]);
        }

        private double borderThickness = DefaultBorderThickness;
        public double BorderThickness
        {
            get => borderThickness;
            set => Set(ref borderThickness, value, etcChangedPropertyNames: AppearancePropertyNames);
        }

        private double cornerRadius = DefaultCornerRadius;
        public double CornerRadius
        {
            get => cornerRadius;
            set => Set(ref cornerRadius, value, etcChangedPropertyNames: AppearancePropertyNames);
        }

        [JsonIgnore]
        public Color BackgroundColor
        {
            get => ColorHelper.Parse(BackgroundColorHex, Colors.Transparent);
            set => BackgroundColorHex = value.ToString();
        }

        [JsonIgnore]
        public Color BorderColor
        {
            get => ColorHelper.Parse(BorderColorHex, Colors.Gray);
            set => BorderColorHex = value.ToString();
        }

        [JsonIgnore]
        public Color ForegroundColor
        {
            get => ColorHelper.Parse(ForegroundColorHex, Colors.White);
            set => ForegroundColorHex = value.ToString();
        }

        [JsonIgnore]
        public Brush AppearanceBackground =>
            UseCustomAppearance ? ColorHelper.Freeze(BackgroundColor) : Brushes.Transparent;

        [JsonIgnore]
        public Brush AppearanceBorderBrush =>
            UseCustomAppearance ? ColorHelper.Freeze(BorderColor) : Brushes.Transparent;

        [JsonIgnore]
        public Thickness AppearanceBorderThickness =>
            UseCustomAppearance ? new Thickness(Math.Max(0, BorderThickness)) : new Thickness(0);

        [JsonIgnore]
        public CornerRadius AppearanceCornerRadius =>
            UseCustomAppearance ? new CornerRadius(Math.Max(0, CornerRadius)) : new CornerRadius(0);

        [JsonIgnore]
        public Brush? AppearanceForeground =>
            UseCustomAppearance ? ColorHelper.Freeze(ForegroundColor) : null;

        public void ResetAppearance()
        {
            BackgroundColorHex = DefaultBackgroundColorHex;
            BorderColorHex = DefaultBorderColorHex;
            ForegroundColorHex = DefaultForegroundColorHex;
            BorderThickness = DefaultBorderThickness;
            CornerRadius = DefaultCornerRadius;
            UseCustomAppearance = false;
        }

        private const string DefaultBackgroundColorHex = "#00000000";
        private const string DefaultBorderColorHex = "#FF808080";
        private const string DefaultForegroundColorHex = "#FFFFFFFF";
        private const double DefaultBorderThickness = 1;
        private const double DefaultCornerRadius = 0;

        private static readonly string[] AppearancePropertyNames =
        [
            nameof(AppearanceBackground),
            nameof(AppearanceBorderBrush),
            nameof(AppearanceBorderThickness),
            nameof(AppearanceCornerRadius),
            nameof(AppearanceForeground),
        ];

        #endregion

        [JsonIgnore]
        public virtual ICommand? DefaultCommand => null;

        [JsonIgnore]
        public virtual Size MinSize => new(1, 1);

        [JsonIgnore]
        public virtual ItemCategory Category => ItemCategory.None;

        [JsonIgnore]
        public virtual string DisplayName => Name;

        /// <summary>設定画面でアイテム名の下に表示される説明。アドオンでも上書きできる。</summary>
        [JsonIgnore]
        public virtual string Description => string.Empty;

        /// <summary>設定画面の一覧で個体を見分けるための表示名。</summary>
        [JsonIgnore]
        public virtual string InstanceSummary => Label;

        /// <summary>クリックしても何も実行しないアイテムは false にする。</summary>
        [JsonIgnore]
        public virtual bool IsInvokable => true;

        [JsonIgnore]
        public virtual string Name
        {
            get
            {
                var typeName = GetType().Name;
                return typeName.EndsWith("Settings", StringComparison.Ordinal)
                    ? typeName[..^"Settings".Length]
                    : typeName;
            }
        }

        [JsonIgnore]
        public virtual string IconPath => string.Empty;
    }
}
