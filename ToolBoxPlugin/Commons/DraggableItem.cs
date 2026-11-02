using ToolBox.Items;

namespace ToolBox.Commons
{
    public abstract class DraggableItem
    {
        public abstract Type SettingsType { get; }
        public abstract Type ViewType { get; }
        public abstract Type? ViewModelType { get; }
        public abstract Type? SettingsViewType { get; }
        public abstract Type? SettingsViewModelType { get; }

        public abstract IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions();
    }
}
