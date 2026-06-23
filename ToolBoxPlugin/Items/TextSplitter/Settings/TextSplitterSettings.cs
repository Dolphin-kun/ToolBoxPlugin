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

        [JsonIgnore]
        public override Size MinSize => new(1, 1);

        [JsonIgnore]
        public override ItemCategory Category => ItemCategory.Text;

        [JsonIgnore]
        public override string Name => "TextSplitter";

        [JsonIgnore]
        public override string DisplayName => "テキスト分割";

        [JsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("SplitTextCommand");
    }
}
