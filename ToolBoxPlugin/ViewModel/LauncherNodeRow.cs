using System.Windows;
using ToolBox.Settings;

namespace ToolBox.ViewModel
{
    public sealed record LauncherNodeRow(LauncherNode Node, int Depth)
    {
        private const double IndentWidth = 16;

        public Thickness Indent => new(Depth * IndentWidth, 0, 0, 0);
    }
}
