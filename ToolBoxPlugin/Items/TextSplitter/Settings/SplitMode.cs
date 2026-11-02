using System.ComponentModel.DataAnnotations;

namespace ToolBox.Items.TextSplitter.Settings
{
    public enum SplitMode
    {
        [Display(Name = nameof(Texts.SplitModeByCharacter), Description = nameof(Texts.SplitModeByCharacterDescription), ResourceType = typeof(Texts))]
        PerCharacter,

        [Display(Name = nameof(Texts.SplitModeByLine), Description = nameof(Texts.SplitModeByLineDescription), ResourceType = typeof(Texts))]
        PerLine,
    }
}
