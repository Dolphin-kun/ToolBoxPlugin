using ToolBox.Commons;
using ToolBox.Items.ItemPreset.Settings;
using ToolBox.Items.ItemPreset.Views;

namespace ToolBox.Items.ItemPreset
{
    public sealed class ItemPreset : DraggableItem
    {
        public override Type SettingsType => typeof(ItemPresetSettings);
        public override Type ViewType => typeof(ItemPresetItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(ItemPresetSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("AddPresetItemCommand",
                (ctx, param) => param is ItemPresetSettings settings && ItemPresetCommand.CanAdd(ctx, settings),
                (ctx, param) =>
                {
                    if (param is ItemPresetSettings settings)
                        ItemPresetCommand.Add(ctx, settings);
                },
                displayName: "保存したアイテムを追加",
                requiresItemParameter: true),
            new("SaveItemStockCommand",
                (ctx, param) => param is ItemPresetSettings && ItemPresetCommand.CanSave(ctx),
                (ctx, param) =>
                {
                    if (param is ItemPresetSettings settings)
                        ItemPresetCommand.SaveAsNewStock(ctx, settings);
                },
                displayName: "選んでいるアイテムを保存して紐づける",
                requiresItemParameter: true),
            new("OverwriteItemStockCommand",
                (ctx, param) => param is ItemPresetSettings settings
                                && settings.HasStock
                                && ItemPresetCommand.CanSave(ctx),
                (ctx, param) =>
                {
                    if (param is ItemPresetSettings settings)
                        ItemPresetCommand.Overwrite(ctx, settings);
                },
                displayName: "紐づけ先を選んでいるアイテムで上書きする",
                requiresItemParameter: true),
        ];
    }
}
