using System.ComponentModel.DataAnnotations;

namespace ToolBox.Items.TextSplitter.Settings
{
    public enum SplitMode
    {
        [Display(Name = "1文字ごとに分割", Description = "1文字ごとに分割")]
        PerCharacter,

        [Display(Name = "改行で分割", Description = "改行で分割")]
        PerLine,
    }
}
