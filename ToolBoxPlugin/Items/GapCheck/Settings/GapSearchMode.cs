using System.ComponentModel.DataAnnotations;

namespace ToolBox.Items.GapCheck.Settings
{
    public enum GapSearchMode
    {
        [Display(Name = nameof(Texts.GapCheckModeAll), ResourceType = typeof(Texts))]
        All,
        [Display(Name = nameof(Texts.GapCheckModePerLayer), ResourceType = typeof(Texts))]
        PerLayer,
        [Display(Name = nameof(Texts.GapCheckModeAcrossLayers), ResourceType = typeof(Texts))]
        AcrossLayers,
    }
}
