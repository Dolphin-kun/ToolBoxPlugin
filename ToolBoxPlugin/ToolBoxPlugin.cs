using ToolBox.Commons;
using ToolBox.View;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Plugin;

namespace ToolBox
{
    [PluginDetails(AuthorName = "いるかぁぁ", ContentId = "nc496573")]
    public class ToolBoxPlugin : IToolPlugin
    {
        public ToolBoxPlugin() => RadialLauncher.Install();

        public string Name => Texts.PluginName;
        public Type ViewModelType => typeof(ToolBoxViewModel);
        public Type ViewType => typeof(ToolBoxView);
        public bool AllowMultipleInstances => true;
    }
}
