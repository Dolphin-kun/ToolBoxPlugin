using ToolBox.Commons;
using ToolBox.Items.Decorations.Settings;
using ToolBox.Items.Decorations.Views;

namespace ToolBox.Items.Decorations
{
    public sealed class LabelDecoration : DraggableItem
    {
        public override Type SettingsType => typeof(LabelSettings);
        public override Type ViewType => typeof(LabelItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(LabelSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() => [];
    }
}
