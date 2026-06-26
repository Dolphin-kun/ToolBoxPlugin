using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;

namespace ToolBox.Items.TextSplitter.Settings
{
    public class TextSplitterSettings : ToolItemSettings
    {
        private bool isDeleteOriginalItem = false;
        public bool IsDeleteOriginalItem { get => isDeleteOriginalItem; set => Set(ref isDeleteOriginalItem, value); }

        private SplitDirection splitDirection = SplitDirection.Vertical;
        public SplitDirection SplitDirection { get => splitDirection; set => Set(ref splitDirection, value); }

        private SplitMode splitMode = SplitMode.PerCharacter;
        public SplitMode SplitMode { get => splitMode; set => Set(ref splitMode, value); }

        private int frameOffset = 0;
        public int FrameOffset { get => frameOffset; set => Set(ref frameOffset, Math.Max(0, value)); }

        private bool isKeepLength = false;
        public bool IsKeepLength { get => isKeepLength; set => Set(ref isKeepLength, value); }

        private bool isKeepPosition = true;
        public bool IsKeepPosition { get => isKeepPosition; set => Set(ref isKeepPosition, value); }

        private bool isIgnoreTags = true;
        public bool IsIgnoreTags { get => isIgnoreTags; set => Set(ref isIgnoreTags, value); }

        [JsonIgnore]
        public override Size MinSize => new(1, 1);

        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Text;

        [JsonIgnore]
        public override string Name => "TextSplitter";

        [JsonIgnore]
        public override string IconPath => "M1.5 9H3.5V15H5.5V9H7.5V7H1.5zM10 7H11L9 15H8L10 7M12 7H13.5L15 9.5L16.5 7H18L15.8 11L18 15H16.5L15 12.5L13.5 15H12L14.2 11L12 7z";

        [JsonIgnore]
        public override string DisplayName => "テキスト分割";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("TextSplitterItemCommand");
    }
}
