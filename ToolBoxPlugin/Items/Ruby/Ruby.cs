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
        public override string IconPath => "M16 4H18L23 18H20.8L19.5 14H14.5L13.2 18H11M15.2 12H18.8L17 7M2.8 14H6.8L5.5 10.2M8.1 18H5.9L4.8 14.8H1.3L0.2 18H-2L2.4 6H4.6M7 4V2H10V4M10 6V4H13V6";

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("AddRubyItemCommand",
                ctx => RubyCommand.CanExecute(ctx.SelectedItems ?? []),
                ctx => _ = RubyCommand.AddRuby(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
