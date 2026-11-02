using ToolBox.Commons;
using ToolBox.Items.ColorPalette.Settings;
using ToolBox.Items.ColorPalette.Views;

namespace ToolBox.Items.ColorPalette
{
    public sealed class ColorPalette : DraggableItem
    {
        public override Type SettingsType => typeof(ColorPaletteSettings);
        public override Type ViewType => typeof(ColorPaletteItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() => [];
    }
}
