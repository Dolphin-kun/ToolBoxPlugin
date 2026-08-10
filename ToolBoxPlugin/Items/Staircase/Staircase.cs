using ToolBox.Commons;
using ToolBox.Items.Staircase.Settings;
using ToolBox.Items.Staircase.Views;

namespace ToolBox.Items.Staircase
{
    public sealed class Staircase : DraggableItem
    {
        public override Type SettingsType => typeof(StaircaseSettings);
        public override Type ViewType => typeof(StaircaseItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(StaircaseSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("StaircaseItemCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => StaircaseItemCommand.Staircase(ctx, ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager),
                displayName: "階段状に配置"),
            new("ReverseStaircaseItemCommand",
                ctx => ctx.SelectedItems?.Any() == true,
                ctx => StaircaseItemCommand.ReverseStaircase(ctx, ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager),
                displayName: "逆向きに階段状に配置"),
        ];
    }
}
