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
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("StaircaseItemCommand",
                StaircaseItemCommand.CanExecute,
                ctx => StaircaseItemCommand.Staircase(ctx, reverse: false),
                displayName: Texts.StaircaseDisplayName),
            new("ReverseStaircaseItemCommand",
                StaircaseItemCommand.CanExecute,
                ctx => StaircaseItemCommand.Staircase(ctx, reverse: true),
                displayName: Texts.StaircaseReverseCommandName),
        ];
    }
}
