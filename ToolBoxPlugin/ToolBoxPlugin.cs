using ToolBox.View;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Plugin;

namespace ToolBox
{
    public class ToolBoxPlugin : IToolPlugin
    {
        public string Name => "ツールボックス";
        public Type ViewModelType => typeof(ToolBoxViewModel);
        public Type ViewType => typeof(ToolBoxView);
    }
}
