using System.Reflection;
using System.Windows;
using SampleAddon.Views;
using ToolBox.Commons;
using ToolBox.Items;
using ToolBox.Settings;

namespace SampleAddon;

public sealed class SampleAddonPlugin : IToolBoxAddon
{
    public string Name => "Sample Addon";
    public string Description => "サンプル用の追加アドオンです。";
    public string Author => "いるかぁぁ";
    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    /// <summary>
    /// 「アドオンを探す」で導入済みかどうかを判定するために使われる。
    /// 公開するときはリポジトリに ymm4-toolbox-addon トピックを付けたうえで、ここに URL を書く。
    /// </summary>
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
                _ => MessageBox.Show("サンプルアドオンです。", "Sample Addon", MessageBoxButton.OK))
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
