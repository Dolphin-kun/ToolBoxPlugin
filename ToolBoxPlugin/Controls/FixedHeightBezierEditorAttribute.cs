using System.Windows;
using YukkuriMovieMaker.Controls;

namespace ToolBox.Controls
{
    public sealed class FixedHeightBezierEditorAttribute(double height) : BezierAnimationEditorAttribute
    {
        public override FrameworkElement Create()
        {
            var editor = base.Create();
            editor.Height = height;
            return editor;
        }
    }
}
