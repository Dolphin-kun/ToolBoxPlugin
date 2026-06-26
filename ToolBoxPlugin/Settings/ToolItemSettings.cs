using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
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

        [JsonIgnore]
        public virtual ICommand? DefaultCommand => null;

        [JsonIgnore]
        public virtual Size MinSize => new(1, 1);

        [JsonIgnore]
        public virtual ItemCategory Category => ItemCategory.None;

        [JsonIgnore]
        public virtual string DisplayName => Name;

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
