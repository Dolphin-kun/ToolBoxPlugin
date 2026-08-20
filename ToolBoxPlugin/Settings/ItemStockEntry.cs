using System.Text.Json.Serialization;
using YukkuriMovieMaker.Commons;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Settings
{
    public class ItemStockEntry : Bindable
    {
        private Guid id = Guid.NewGuid();
        public Guid Id
        {
            get => id;
            set => id = value == Guid.Empty ? Guid.NewGuid() : value;
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => Set(ref name, value ?? string.Empty, etcChangedPropertyNames: nameof(DisplayLabel));
        }

        private string itemsJson = string.Empty;
        public string ItemsJson
        {
            get => itemsJson;
            set => Set(ref itemsJson, value ?? string.Empty);
        }

        private string summary = string.Empty;
        public string Summary
        {
            get => summary;
            set => Set(ref summary, value ?? string.Empty, etcChangedPropertyNames: nameof(DisplayLabel));
        }

        private int itemCount = 1;
        public int ItemCount
        {
            get => itemCount;
            set => Set(ref itemCount, Math.Max(0, value), etcChangedPropertyNames: nameof(DisplayLabel));
        }

        private bool isEditing;
        [JsonIgnore, NJsonIgnore]
        public bool IsEditing
        {
            get => isEditing;
            set => Set(ref isEditing, value);
        }

        [JsonIgnore, NJsonIgnore]
        public string DisplayLabel => string.IsNullOrWhiteSpace(Name) ? Summary : Name;
    }
}
