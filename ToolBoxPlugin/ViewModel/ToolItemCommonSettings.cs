using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using ToolBox.Controls;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.ItemEditor.CustomVisibilityAttributes;
using YukkuriMovieMaker.Settings;

namespace ToolBox.ViewModel
{
    public sealed class ToolItemCommonSettings : Bindable, IDisposable
    {
        private readonly ToolItemSettings settings;

        public ToolItemCommonSettings(ToolItemSettings settings)
        {
            this.settings = settings;
            settings.PropertyChanged += OnSettingsPropertyChanged;
        }

        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName ?? string.Empty);

            if (e.PropertyName == nameof(ToolItemSettings.ImageStretch))
                OnPropertyChanged(nameof(ImageFit));

            if (e.PropertyName == nameof(ToolItemSettings.DisplayMode))
                OnPropertyChanged(nameof(IsImageMode));
        }

        public void Dispose() => settings.PropertyChanged -= OnSettingsPropertyChanged;

        public bool IsInvokable => settings.IsInvokable;

        public bool IsImageMode => settings.DisplayMode == ToolItemDisplayMode.Image;

        [Display(GroupName = nameof(Texts.ItemCommonDisplaySection), Name = nameof(Texts.ItemCommonButtonLabel), Order = 0, ResourceType = typeof(Texts))]
        [TextEditor(AcceptsReturn = false, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public string Label { get => settings.Label; set => settings.Label = value; }

        [Display(GroupName = nameof(Texts.ItemCommonDisplaySection), Name = nameof(Texts.ItemCommonDisplayModeLabel), Order = 1, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public ToolItemDisplayMode DisplayMode { get => settings.DisplayMode; set => settings.DisplayMode = value; }

        [Display(GroupName = nameof(Texts.ItemCommonDisplaySection), Name = nameof(Texts.ItemCommonImageFileLabel), Order = 2, ResourceType = typeof(Texts))]
        [FileSelector(FileGroupType.ImageItem, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(IsImageMode), true)]
        public string CustomImagePath { get => settings.CustomImagePath; set => settings.CustomImagePath = value; }

        [Display(GroupName = nameof(Texts.ItemCommonDisplaySection), Name = nameof(Texts.ItemCommonImageFitLabel), Order = 3, ResourceType = typeof(Texts))]
        [EnumComboBox(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(IsImageMode), true)]
        public ToolItemImageFit ImageFit
        {
            get => settings.ImageStretch switch
            {
                Stretch.Fill => ToolItemImageFit.Fill,
                Stretch.UniformToFill => ToolItemImageFit.UniformToFill,
                _ => ToolItemImageFit.Uniform,
            };
            set => settings.ImageStretch = value switch
            {
                ToolItemImageFit.Fill => Stretch.Fill,
                ToolItemImageFit.UniformToFill => Stretch.UniformToFill,
                _ => Stretch.Uniform,
            };
        }

        [Display(GroupName = nameof(Texts.ItemCommonColorSection), Name = nameof(Texts.ItemCommonUseCustomColor), Order = 10, ResourceType = typeof(Texts))]
        [ToggleSlider(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        public bool UseCustomAppearance { get => settings.UseCustomAppearance; set => settings.UseCustomAppearance = value; }

        [Display(GroupName = nameof(Texts.ItemCommonColorSection), Name = nameof(Texts.ItemCommonBackground), Order = 11, ResourceType = typeof(Texts))]
        [ColorPicker(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(UseCustomAppearance), true)]
        public Color BackgroundColor { get => settings.BackgroundColor; set => settings.BackgroundColor = value; }

        [Display(GroupName = nameof(Texts.ItemCommonColorSection), Name = nameof(Texts.ItemCommonBorder), Order = 12, ResourceType = typeof(Texts))]
        [ColorPicker(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(UseCustomAppearance), true)]
        public Color BorderColor { get => settings.BorderColor; set => settings.BorderColor = value; }

        [Display(GroupName = nameof(Texts.ItemCommonColorSection), Name = nameof(Texts.ItemCommonForeground), Order = 13, ResourceType = typeof(Texts))]
        [ColorPicker(PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(UseCustomAppearance), true)]
        public Color ForegroundColor { get => settings.ForegroundColor; set => settings.ForegroundColor = value; }

        [Display(GroupName = nameof(Texts.ItemCommonColorSection), Name = nameof(Texts.ItemCommonBorderThickness), Order = 14, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "px", 0, 8, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(UseCustomAppearance), true)]
        public double BorderThickness { get => settings.BorderThickness; set => settings.BorderThickness = value; }

        [Display(GroupName = nameof(Texts.ItemCommonColorSection), Name = nameof(Texts.ItemCommonCornerRadius), Order = 15, ResourceType = typeof(Texts))]
        [TextBoxSlider("F0", "px", 0, 20, PropertyEditorSize = PropertyEditorSize.FullWidth)]
        [ShowPropertyEditorWhen(nameof(UseCustomAppearance), true)]
        public double CornerRadius { get => settings.CornerRadius; set => settings.CornerRadius = value; }

        [Display(GroupName = nameof(Texts.ItemCommonHotkey), Name = nameof(Texts.ItemCommonHotkey), Description = nameof(Texts.ItemCommonHotkeyHint), Order = 20, ResourceType = typeof(Texts))]
        [HotkeyEditor]
        [ShowPropertyEditorWhen(nameof(IsInvokable), true)]
        public string HotkeyGesture { get => settings.HotkeyGesture; set => settings.HotkeyGesture = value; }
    }
}
