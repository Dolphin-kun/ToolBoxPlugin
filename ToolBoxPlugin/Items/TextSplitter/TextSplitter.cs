using ToolBox.Items.TextSplitter.Settings;
using ToolBox.Items.TextSplitter.Views;

namespace ToolBox.Items.TextSplitter
{
    public sealed class TextSplitter : DraggableItem
    {
        public override Type SettingsType => typeof(TextSplitterSettings);
        public override Type ViewType => typeof(TextSplitterItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(TextSplitterSettingsPanel);
        public override Type? SettingsViewModelType => null;
        public override string IconPath => "M0,0 L24,24 M0,24 L24,0";

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("SplitTextCommand",
                ctx => TextSplitterCommand.CanExecute(ctx.SelectedItems ?? []),
                ctx => TextSplitterCommand.SplitText(ctx.SelectedItems ?? [], ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
