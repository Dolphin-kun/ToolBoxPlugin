using ToolBox.Commons;
using ToolBox.Items.SceneMover.Settings;
using ToolBox.Items.SceneMover.Views;

namespace ToolBox.Items.SceneMover
{
    public sealed class SceneMover : DraggableItem
    {
        public override Type SettingsType => typeof(SceneMoverSettings);
        public override Type ViewType => typeof(SceneMoverItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(SceneMoverSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("MoveToNewSceneCommand",
                (ctx, _) => SceneMoverCommand.CanMoveToNewScene(ctx),
                (ctx, param) => SceneMoverCommand.MoveToNewScene(ctx, param as SceneMoverSettings),
                displayName: Texts.SceneMoverToNewSceneCommandName),
            new("MoveToSceneCommand",
                (ctx, param) => param is SceneMoveTarget && SceneMoverCommand.CanMove(ctx),
                (ctx, param) =>
                {
                    if (param is SceneMoveTarget target)
                        SceneMoverCommand.MoveToScene(ctx, target.Settings, target.Target);
                },
                displayName: Texts.SceneMoverToSceneCommandName,
                requiresItemParameter: true),
        ];
    }
}
