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

namespace ToolBox.Items.KeyFrameTools.Settings
{
    public class KeyFrameToolsSettings : ToolItemSettings
    {
        private int shiftFrames = 5;
        [Display(GroupName = nameof(Texts.KeyFrameToolsDisplayName), Name = nameof(Texts.KeyFrameShiftFramesLabel), Order = 0, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "F", 1, 30, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public int ShiftFrames { get => shiftFrames; set => Set(ref shiftFrames, Math.Max(1, value)); }

        private bool copyReversed;
        [Display(GroupName = nameof(Texts.KeyFrameToolsDisplayName), Name = nameof(Texts.KeyFrameCopyReversedLabel), Description = nameof(Texts.KeyFrameCopyReversedDescription), Order = 1, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool CopyReversed { get => copyReversed; set => Set(ref copyReversed, value); }

        public KeyFrameToolsSettings()
        {
            Size = new Size(3, 1);
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Animation;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "KeyFrameTools";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.SwapHorizontal;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.KeyFrameToolsDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.KeyFrameToolsDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("KeyFrameReverseCommand");
    }
}
