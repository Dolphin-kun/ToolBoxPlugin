using ToolBox.Settings;

namespace ToolBox.Commons
{
    public interface IToolBoxAddon
    {
        string Name { get; }
        string Description { get; }
        string Author { get; }
        string Version { get; }
        string? RepositoryUrl { get; }
        IReadOnlyList<DraggableItem> GetDraggableItems();
        IReadOnlyList<ToolItemSettings> GetSettings();
    }
}
