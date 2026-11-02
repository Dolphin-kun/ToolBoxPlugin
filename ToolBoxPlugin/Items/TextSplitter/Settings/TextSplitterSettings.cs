using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.ItemEditor.CustomVisibilityAttributes;
using YukkuriMovieMaker.Controls;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.TextSplitter.Settings
{
    public class TextSplitterSettings : ToolItemSettings
    {
        private bool isDeleteOriginalItem = false;
        [Display(GroupName = nameof(Texts.TextSplitterDisplayName), Name = nameof(Texts.TextSplitterDeleteOriginal), Order = 0, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool IsDeleteOriginalItem { get => isDeleteOriginalItem; set => Set(ref isDeleteOriginalItem, value); }

        private SplitDirection splitDirection = SplitDirection.Vertical;
        [Display(GroupName = nameof(Texts.TextSplitterDisplayName), Name = nameof(Texts.TextSplitterDirectionLabel), Order = 2, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public SplitDirection SplitDirection { get => splitDirection; set => Set(ref splitDirection, value); }

        private SplitMode splitMode = SplitMode.PerCharacter;
        [Display(GroupName = nameof(Texts.TextSplitterDisplayName), Name = nameof(Texts.TextSplitterModeLabel), Order = 1, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public SplitMode SplitMode { get => splitMode; set => Set(ref splitMode, value); }

        private int frameOffset = 0;
        [Display(GroupName = nameof(Texts.TextSplitterDisplayName), Name = nameof(Texts.TextSplitterOffsetLabel), Order = 5, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "F", 0, 300, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public int FrameOffset { get => frameOffset; set => Set(ref frameOffset, Math.Max(0, value)); }

        private bool isKeepLength = false;
        [Display(GroupName = nameof(Texts.TextSplitterDisplayName), Name = nameof(Texts.TextSplitterKeepDuration), Order = 6, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool IsKeepLength { get => isKeepLength; set => Set(ref isKeepLength, value); }

        private bool isKeepPosition = true;
        [Display(GroupName = nameof(Texts.TextSplitterDisplayName), Name = nameof(Texts.TextSplitterKeepPosition), Order = 3, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool IsKeepPosition { get => isKeepPosition; set => Set(ref isKeepPosition, value); }

        private bool isIgnoreTags = true;
        [Display(GroupName = nameof(Texts.TextSplitterDisplayName), Name = nameof(Texts.TextSplitterIgnoreTags), Description = nameof(Texts.TextSplitterIgnoreTagsTip), Order = 4, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool IsIgnoreTags { get => isIgnoreTags; set => Set(ref isIgnoreTags, value); }

        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(1, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Text;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "TextSplitter";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => "M1.5 9H3.5V15H5.5V9H7.5V7H1.5zM10 7H11L9 15H8L10 7M12 7H13.5L15 9.5L16.5 7H18L15.8 11L18 15H16.5L15 12.5L13.5 15H12L14.2 11L12 7z";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.TextSplitterDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.TextSplitterDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("TextSplitterItemCommand");
    }
}
