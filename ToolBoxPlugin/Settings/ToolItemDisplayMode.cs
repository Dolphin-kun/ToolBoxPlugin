using System.ComponentModel.DataAnnotations;

namespace ToolBox.Settings
{
    public enum ToolItemDisplayMode
    {
        [Display(Name = nameof(Texts.DisplayModeIcon), ResourceType = typeof(Texts))]
        Icon,
        [Display(Name = nameof(Texts.DisplayModeText), ResourceType = typeof(Texts))]
        Text,
        [Display(Name = nameof(Texts.DisplayModeImage), ResourceType = typeof(Texts))]
        Image
    }

    public enum ToolItemImageFit
    {
        [Display(Name = nameof(Texts.ImageFitUniform), ResourceType = typeof(Texts))]
        Uniform,
        [Display(Name = nameof(Texts.ImageFitFill), ResourceType = typeof(Texts))]
        Fill,
        [Display(Name = nameof(Texts.ImageFitUniformToFill), ResourceType = typeof(Texts))]
        UniformToFill,
    }
}
