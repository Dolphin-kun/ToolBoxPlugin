using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Macro.Settings
{
    public class MacroSettings : ToolItemSettings
    {
        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Utility;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Macro";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.MacroDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description =>
            Texts.MacroDescription;

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.PlaylistPlay;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary =>
            Steps.Count == 0
                ? string.Format(Texts.MacroSummaryNoSteps, Label)
                : string.Format(Texts.MacroSummarySteps, Label, Steps.Count);

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("RunMacroItemCommand");

        private ObservableCollection<MacroStep> steps = [];
        public ObservableCollection<MacroStep> Steps
        {
            get => steps;
            set
            {
                if (ReferenceEquals(steps, value))
                    return;

                steps.CollectionChanged -= OnStepsChanged;
                steps = value ?? [];
                steps.CollectionChanged += OnStepsChanged;

                OnPropertyChanged();
                OnPropertyChanged(nameof(InstanceSummary));
            }
        }

        public MacroSettings()
        {
            steps.CollectionChanged += OnStepsChanged;
        }

        private void OnStepsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(InstanceSummary));
    }
}
