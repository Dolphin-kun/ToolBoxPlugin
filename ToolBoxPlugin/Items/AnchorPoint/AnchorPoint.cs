using ToolBox.Commons;
using ToolBox.Items.AnchorPoint.Settings;
using ToolBox.Items.AnchorPoint.Views;

namespace ToolBox.Items.AnchorPoint
{
    public sealed class AnchorPoint : DraggableItem
    {
        public override Type SettingsType => typeof(AnchorPointSettings);
        public override Type ViewType => typeof(AnchorPointItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            .. PlacementPosition.All.Select(position => new ToolItemCommandDefinition(
                AnchorPointCommand.CommandName(position),
                AnchorPointCommand.CanApply,
                ctx => AnchorPointCommand.Apply(ctx, position),
                displayName: string.Format(Texts.AnchorPointCommandName, position.Label))),
        ];
    }
}
