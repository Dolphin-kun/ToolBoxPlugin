using ToolBox.Items.Ruby.Settings;
using ToolBox.Items.Ruby.Views;

namespace ToolBox.Items.Ruby
{
    public sealed class Ruby : DraggableItem
    {
        public override Type SettingsType => typeof(RubySettings);
        public override Type ViewType => typeof(RubyItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(RubySettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("RubyItemCommand",
                ctx => RubyCommand.CanExecute(ctx.SelectedItems ?? []),
                ctx => _ = RubyCommand.AddRuby(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
