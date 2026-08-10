using ToolBox.Commons;
using ToolBox.Items.Macro.Settings;
using ToolBox.Items.Macro.Views;

namespace ToolBox.Items.Macro
{
    public sealed class Macro : DraggableItem
    {
        public override Type SettingsType => typeof(MacroSettings);
        public override Type ViewType => typeof(MacroItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(MacroSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("RunMacroItemCommand",
                (_, param) => param is MacroSettings settings && MacroCommand.CanRun(settings),
                (_, param) =>
                {
                    if (param is MacroSettings settings)
                        MacroCommand.Run(settings);
                },
                displayName: "マクロを実行",
                requiresItemParameter: true),
        ];
    }
}
