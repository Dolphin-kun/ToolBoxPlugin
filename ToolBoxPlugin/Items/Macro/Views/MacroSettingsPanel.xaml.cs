using System.Windows;
using System.Windows.Controls;
using ToolBox.Commons;
using ToolBox.Items.Macro.Settings;
using ToolBox.Items.Shortcuts;
using ToolBox.ViewModel;

namespace ToolBox.Items.Macro.Views
{
    public partial class MacroSettingsPanel : UserControl
    {
        public IReadOnlyList<CommandCatalogEntry> AvailableCommands { get; }

        public IReadOnlyList<ShortcutDefinition> AvailableShortcuts { get; } = ShortcutDefinitions.Allowed;

        public IReadOnlyList<PanelItemChoice> AvailablePanelItems { get; }

        public MacroSettingsPanel()
        {
            AvailableCommands = [.. CommandCatalog.GetEntries().Where(e => e.CommandName != "RunMacroItemCommand")];

            AvailablePanelItems =
            [
                .. ToolBoxViewModel.GetAllPlacedItems()
                    .Where(item => item.IsInvokable && item is not MacroSettings)
                    .Select(item => new PanelItemChoice(item.Id, $"{item.DisplayName} / {item.InstanceSummary}"))
            ];

            InitializeComponent();
        }

        public record PanelItemChoice(Guid Id, string DisplayText);

        private MacroSettings? Settings => DataContext as MacroSettings;

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            if (Settings == null)
                return;

            var step = new MacroStep { Kind = MacroStepKind.Command };
            var insertAt = StepList.SelectedIndex >= 0 ? StepList.SelectedIndex + 1 : Settings.Steps.Count;

            Settings.Steps.Insert(insertAt, step);
            StepList.SelectedItem = step;
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (Settings == null || StepList.SelectedItem is not MacroStep step)
                return;

            var index = Settings.Steps.IndexOf(step);
            Settings.Steps.Remove(step);
            StepList.SelectedIndex = Math.Min(index, Settings.Steps.Count - 1);
        }

        private void OnMoveUpClick(object sender, RoutedEventArgs e) => Move(-1);

        private void OnMoveDownClick(object sender, RoutedEventArgs e) => Move(1);

        private void Move(int offset)
        {
            if (Settings == null || StepList.SelectedItem is not MacroStep step)
                return;

            var index = Settings.Steps.IndexOf(step);
            var target = index + offset;
            if (index < 0 || target < 0 || target >= Settings.Steps.Count)
                return;

            Settings.Steps.Move(index, target);
            StepList.SelectedIndex = target;
        }
    }
}
