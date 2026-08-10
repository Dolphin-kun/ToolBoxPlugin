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
        public override string DisplayName => "マクロ";

        [JsonIgnore, NJsonIgnore]
        public override string Description =>
            "登録した操作を上から順に実行します。よく使う手順をまとめて 1 つのボタンにできます。";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.PlaylistPlay;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary =>
            Steps.Count == 0 ? $"{Label} (手順なし)" : $"{Label} ({Steps.Count} 手順)";

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
            Label = "マクロ";
            steps.CollectionChanged += OnStepsChanged;
        }

        private void OnStepsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            => OnPropertyChanged(nameof(InstanceSummary));
    }
}
