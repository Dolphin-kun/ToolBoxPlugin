using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Settings
{
    [JsonConverter(typeof(ToolItemSettingsConverter))]
    public class ToolItemSettings : Bindable
    {
        /// <summary>配置された 1 個ごとに割り振られる識別子。マクロの参照先などに使う。</summary>
        private Guid id = Guid.NewGuid();
        public Guid Id
        {
            get => id;
            set => id = value == Guid.Empty ? Guid.NewGuid() : value;
        }

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

        [JsonIgnore, NJsonIgnore]
        public Color BackgroundColor
        {
            get => ColorHelper.Parse(BackgroundColorHex, Colors.Transparent);
            set => BackgroundColorHex = value.ToString();
        }

        [JsonIgnore, NJsonIgnore]
        public Color BorderColor
        {
            get => ColorHelper.Parse(BorderColorHex, Colors.Gray);
            set => BorderColorHex = value.ToString();
        }

        [JsonIgnore, NJsonIgnore]
        public Color ForegroundColor
        {
            get => ColorHelper.Parse(ForegroundColorHex, Colors.White);
            set => ForegroundColorHex = value.ToString();
        }

        [JsonIgnore, NJsonIgnore]
        public Brush AppearanceBackground =>
            UseCustomAppearance ? ColorHelper.Freeze(BackgroundColor) : Brushes.Transparent;

        [JsonIgnore, NJsonIgnore]
        public Brush AppearanceBorderBrush =>
            UseCustomAppearance ? ColorHelper.Freeze(BorderColor) : Brushes.Transparent;

        [JsonIgnore, NJsonIgnore]
        public Thickness AppearanceBorderThickness =>
            UseCustomAppearance ? new Thickness(Math.Max(0, BorderThickness)) : new Thickness(0);

        [JsonIgnore, NJsonIgnore]
        public CornerRadius AppearanceCornerRadius =>
            UseCustomAppearance ? new CornerRadius(Math.Max(0, CornerRadius)) : new CornerRadius(0);

        [JsonIgnore, NJsonIgnore]
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

        [JsonIgnore, NJsonIgnore]
        public virtual ICommand? DefaultCommand => null;

        [JsonIgnore, NJsonIgnore]
        public virtual Size MinSize => new(1, 1);

        [JsonIgnore, NJsonIgnore]
        public virtual ItemCategory Category => ItemCategory.None;

        [JsonIgnore, NJsonIgnore]
        public virtual string DisplayName => Name;

        /// <summary>設定画面でアイテム名の下に表示される説明。アドオンでも上書きできる。</summary>
        [JsonIgnore, NJsonIgnore]
        public virtual string Description => string.Empty;

        /// <summary>非表示にしたアイテムを控えておくときの区分。同じ種類なら同じ値を返すこと。</summary>
        [JsonIgnore, NJsonIgnore]
        public virtual string HiddenKey => GetType().FullName ?? GetType().Name;

        /// <summary>設定画面の一覧で個体を見分けるための表示名。</summary>
        [JsonIgnore, NJsonIgnore]
        public virtual string InstanceSummary => Label;

        /// <summary>クリックしても何も実行しないアイテムは false にする。</summary>
        [JsonIgnore, NJsonIgnore]
        public virtual bool IsInvokable => true;

        [JsonIgnore, NJsonIgnore]
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

        [JsonIgnore, NJsonIgnore]
        public virtual string IconPath => string.Empty;
    }
}
