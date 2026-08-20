using System.Reflection;
using System.Windows;
using SampleAddon.Views;
using ToolBox.Commons;
using ToolBox.Items;
using ToolBox.Settings;

namespace SampleAddon;

public sealed class SampleAddonPlugin : IToolBoxAddon
{
    public string Name => "サンプルアドオン";
    public string Description => "サンプル用の追加アドオンです。";
    public string Author => "いるかぁぁ";
    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

    public string? RepositoryUrl => null;

    public IReadOnlyList<DraggableItem> GetDraggableItems() =>
    [
        new SampleHelloItem()
    ];

    public IReadOnlyList<ToolItemSettings> GetSettings() =>
    [
        new SampleHelloSettings()
    ];

    public sealed class SampleHelloItem : DraggableItem
    {
        public override Type SettingsType => typeof(SampleHelloSettings);
        public override Type ViewType => typeof(SampleHelloItemView);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => null;
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("ShowMessageItemCommand",
                _ => true,
                _ => MessageBox.Show("サンプルアドオンです。", "サンプルアドオン", MessageBoxButton.OK),
                displayName: "メッセージを表示")
        ];
    }

    public sealed class SampleHelloSettings : ToolItemSettings
    {
        public override ItemCategory Category => ItemCategory.Text;
        public override string DisplayName => "サンプルアドオン";
        public override string Description => "クリックするとメッセージを表示する、アドオンの実装サンプルです。";
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.MessageTextOutline;
    }
}
