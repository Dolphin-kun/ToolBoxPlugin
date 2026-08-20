using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Settings;
using ToolBox.ViewModel;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.ItemPreset.Settings
{
    public class ItemPresetSettings : ToolItemSettings
    {
        private Guid stockId;
        public Guid StockId
        {
            get => stockId;
            set => Set(ref stockId, value,
                etcChangedPropertyNames: [nameof(Stock), nameof(HasStock), nameof(InstanceSummary), nameof(IconPath)]);
        }

        private bool useCurrentFrame = true;
        public bool UseCurrentFrame
        {
            get => useCurrentFrame;
            set => Set(ref useCurrentFrame, value);
        }

        private bool useCurrentLayer = true;
        public bool UseCurrentLayer
        {
            get => useCurrentLayer;
            set => Set(ref useCurrentLayer, value);
        }

        private bool selectAfterAdd = true;
        public bool SelectAfterAdd
        {
            get => selectAfterAdd;
            set => Set(ref selectAfterAdd, value);
        }

        [JsonIgnore, NJsonIgnore]
        public ItemStockEntry? Stock => ItemStockStore.Find(StockId);

        [JsonIgnore, NJsonIgnore]
        public bool HasStock => Stock != null;

        [JsonIgnore, NJsonIgnore]
        public ObservableCollection<ItemStockEntry> AvailableStocks => ItemStockStore.Entries;

        [JsonIgnore, NJsonIgnore]
        public ICommand? SaveStockCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("SaveItemStockCommand");

        [JsonIgnore, NJsonIgnore]
        public ICommand? OverwriteStockCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("OverwriteItemStockCommand");

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Utility;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "ItemPreset";

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => "保存したアイテムを追加";

        [JsonIgnore, NJsonIgnore]
        public override string Description =>
            "「保存したアイテム」に貯めておいたアイテムのうち 1 つをこのボタンに紐づけて、クリック 1 回でタイムラインに追加します。";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => HasStock
            ? YukkuriMovieMaker.Resources.Icons.IconKeys.Bookmark
            : YukkuriMovieMaker.Resources.Icons.IconKeys.BookmarkOutline;

        [JsonIgnore, NJsonIgnore]
        public override string InstanceSummary => Stock?.DisplayLabel ?? "(未設定)";

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("AddPresetItemCommand");
    }
}
