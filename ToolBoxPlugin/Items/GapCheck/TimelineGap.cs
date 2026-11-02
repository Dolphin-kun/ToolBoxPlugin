using YukkuriMovieMaker.Project.Items;

namespace ToolBox.Items.GapCheck
{
    public sealed record TimelineGap(int? Layer, int Frame, int Length, IItem Before, IItem After)
    {
        public int EndFrame => Frame + Length;
    }
}
