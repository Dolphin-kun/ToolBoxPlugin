using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Items
{
    public interface IToolBoxContext
    {
        IReadOnlyList<IItem>? SelectedItems { get; }
        Timeline? Timeline { get; }
        UndoRedoManager? UndoRedoManager { get; }
    }
}
