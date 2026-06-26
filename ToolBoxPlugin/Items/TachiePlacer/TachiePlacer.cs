using System;
using System.Collections.Generic;
using ToolBox.Items.TachiePlacer.Settings;
using ToolBox.Items.TachiePlacer.Views;

namespace ToolBox.Items.TachiePlacer
{
    public sealed class TachiePlacer : DraggableItem
    {
        public override Type SettingsType => typeof(TachiePlacerSettings);
        public override Type ViewType => typeof(TachiePlacerItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(TachiePlacerSettingsPanel);
        public override Type? SettingsViewModelType => null;



        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("TachiePlacerItemCommand",
                ctx => TachiePlacerCommand.CanExecute(ctx.SelectedItems ?? []),
                ctx => TachiePlacerCommand.AddTachie(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
