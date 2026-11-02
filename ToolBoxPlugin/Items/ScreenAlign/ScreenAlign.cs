using ToolBox.Commons;
using ToolBox.Items.ScreenAlign.Settings;
using ToolBox.Items.ScreenAlign.Views;

namespace ToolBox.Items.ScreenAlign
{
    public sealed class ScreenAlign : DraggableItem
    {
        public override Type SettingsType => typeof(ScreenAlignSettings);
        public override Type ViewType => typeof(ScreenAlignItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            .. PlacementPosition.All.Select(position => new ToolItemCommandDefinition(
                ScreenAlignCommand.CommandName(position),
                ScreenAlignCommand.CanAlign,
                ctx => ScreenAlignCommand.Align(ctx, position),
                displayName: string.Format(Texts.ScreenAlignCommandName, position.Label))),
            new(ScreenAlignCommand.DistributeHorizontalCommandName,
                ScreenAlignCommand.CanDistribute,
                ctx => ScreenAlignCommand.Distribute(ctx, horizontal: true),
                displayName: Texts.ScreenDistributeHorizontalCommandName),
            new(ScreenAlignCommand.DistributeVerticalCommandName,
                ScreenAlignCommand.CanDistribute,
                ctx => ScreenAlignCommand.Distribute(ctx, horizontal: false),
                displayName: Texts.ScreenDistributeVerticalCommandName),
        ];
    }
}
