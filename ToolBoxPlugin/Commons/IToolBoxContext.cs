using ToolBox.Settings;
using YukkuriMovieMaker.Project;
using YukkuriMovieMaker.Project.Items;
using YukkuriMovieMaker.UndoRedo;

namespace ToolBox.Commons
{
    public interface IToolBoxContext
    {
        IReadOnlyList<IItem>? SelectedItems { get; }
        Timeline? Timeline { get; }
        UndoRedoManager? UndoRedoManager { get; }
        T? FindItemSettings<T>() where T : ToolItemSettings;
    }
}
