using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.MotionCopy.Settings
{
    public class MotionCopySettings : ToolItemSettings
    {
        private bool copyPosition = true;
        [Display(GroupName = nameof(Texts.MotionCopyDisplayName), Name = nameof(Texts.MotionCopyPositionLabel), Order = 0, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool CopyPosition { get => copyPosition; set => Set(ref copyPosition, value); }

        private bool copyOpacity = true;
        [Display(GroupName = nameof(Texts.MotionCopyDisplayName), Name = nameof(Texts.MotionCopyOpacityLabel), Order = 1, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool CopyOpacity { get => copyOpacity; set => Set(ref copyOpacity, value); }

        private bool copyZoom = true;
        [Display(GroupName = nameof(Texts.MotionCopyDisplayName), Name = nameof(Texts.MotionCopyZoomLabel), Order = 2, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool CopyZoom { get => copyZoom; set => Set(ref copyZoom, value); }

        private bool copyRotation = true;
        [Display(GroupName = nameof(Texts.MotionCopyDisplayName), Name = nameof(Texts.MotionCopyRotationLabel), Order = 3, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool CopyRotation { get => copyRotation; set => Set(ref copyRotation, value); }

        private bool keepPosition = true;
        [Display(GroupName = nameof(Texts.MotionCopyDisplayName), Name = nameof(Texts.MotionCopyKeepPositionLabel), Description = nameof(Texts.MotionCopyKeepPositionDescription), Order = 4, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool KeepPosition { get => keepPosition; set => Set(ref keepPosition, value); }

        private bool fitLength = true;
        [Display(GroupName = nameof(Texts.MotionCopyDisplayName), Name = nameof(Texts.MotionCopyFitLengthLabel), Description = nameof(Texts.MotionCopyFitLengthDescription), Order = 5, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool FitLength { get => fitLength; set => Set(ref fitLength, value); }

        public MotionCopySettings()
        {
            Size = new Size(2, 1);
        }

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Animation;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "MotionCopy";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.ContentCopy;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.MotionCopyDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.MotionCopyDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand => ToolBoxViewModel.Instance?.Commands.GetValueOrDefault(MotionCopyCommand.PasteCommandName);
    }
}
