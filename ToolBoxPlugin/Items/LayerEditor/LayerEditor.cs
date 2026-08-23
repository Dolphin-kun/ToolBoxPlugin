using ToolBox.Commons;
using ToolBox.Items.LayerEditor.Settings;
using ToolBox.Items.LayerEditor.Views;

namespace ToolBox.Items.LayerEditor
{
    public sealed class LayerEditor : DraggableItem
    {
        public override Type SettingsType => typeof(LayerEditorSettings);
        public override Type ViewType => typeof(LayerEditorItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("LayerEditorApplyCommand",
                LayerEditorCommand.CanExecute,
                LayerEditorCommand.Open,
                displayName: Texts.LayerEditorApplyCommandName),
            new("LayerEditorResetCommand",
                LayerEditorCommand.CanExecute,
                LayerEditorCommand.Reset,
                displayName: Texts.LayerEditorResetCommandName),
        ];
    }
}
