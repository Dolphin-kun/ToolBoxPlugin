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


        public override string IconPath => "M12,4A4,4 0 0,1 16,8A4,4 0 0,1 12,12A4,4 0 0,1 8,8A4,4 0 0,1 12,4M12,14C16.42,14 20,15.79 20,18V20H4V18C4,15.79 7.58,14 12,14Z";

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("AddTachieItemCommand",
                ctx => TachiePlacerCommand.CanExecute(ctx.SelectedItems ?? []),
                ctx => TachiePlacerCommand.AddTachie(ctx.SelectedItems!, ctx.Timeline, ctx.UndoRedoManager)),
        ];
    }
}
