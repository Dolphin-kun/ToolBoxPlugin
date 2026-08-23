using System.Text.Json.Serialization;
using YukkuriMovieMaker.Commons;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Bookmarks.Settings
{
    public class BookmarkEntry : Bindable
    {
        private int frame;
        public int Frame
        {
            get => frame;
            set => Set(ref frame, Math.Max(0, value), etcChangedPropertyNames: nameof(TimeText));
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => Set(ref name, value ?? string.Empty);
        }

        [JsonIgnore, NJsonIgnore]
        public string TimeText => BookmarkCommand.FormatFrame(frame);
    }
}
