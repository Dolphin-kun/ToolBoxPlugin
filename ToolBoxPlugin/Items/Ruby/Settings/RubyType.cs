using System.ComponentModel.DataAnnotations;

namespace ToolBox.Items.Ruby.Settings
{
    public enum RubyType
    {
        [Display(Name = "ひらがな", Description = "ひらがなでルビを振ります")]
        Hiragana,

        [Display(Name = "カタカナ", Description = "カタカナでルビを振ります")]
        Katakana,
    }
}
