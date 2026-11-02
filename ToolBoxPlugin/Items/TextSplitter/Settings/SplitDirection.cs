using System.ComponentModel.DataAnnotations;

namespace ToolBox.Items.TextSplitter.Settings
{
    public enum SplitDirection
    {
        [Display(Name = nameof(Texts.SplitDirectionVertical), Description = nameof(Texts.SplitDirectionVerticalDescription), ResourceType = typeof(Texts))]
        Vertical,

        [Display(Name = nameof(Texts.SplitDirectionHorizontal), Description = nameof(Texts.SplitDirectionHorizontalDescription), ResourceType = typeof(Texts))]
        Horizontal,
    }
}
