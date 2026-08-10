using ToolBox.Commons;
using ToolBox.Items.Decorations.Settings;
using ToolBox.Items.Decorations.Views;

namespace ToolBox.Items.Decorations
{
    public sealed class SeparatorDecoration : DraggableItem
    {
        public override Type SettingsType => typeof(SeparatorSettings);
        public override Type ViewType => typeof(SeparatorItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(SeparatorSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() => [];
    }
}
