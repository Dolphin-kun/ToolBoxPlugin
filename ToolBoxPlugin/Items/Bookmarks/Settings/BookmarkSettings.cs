using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ToolBox.Settings;
using ToolBox.ViewModel;
using YukkuriMovieMaker.Commons;
using NJsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;

namespace ToolBox.Items.Bookmarks.Settings
{
    public class BookmarkSettings : ToolItemSettings
    {
        private ObservableCollection<BookmarkEntry> bookmarks = [];
        public ObservableCollection<BookmarkEntry> Bookmarks
        {
            get => bookmarks;
            set
            {
                bookmarks.CollectionChanged -= OnBookmarksChanged;
                Set(ref bookmarks, value ?? [], etcChangedPropertyNames: nameof(HasBookmarks));
                bookmarks.CollectionChanged += OnBookmarksChanged;
            }
        }

        [JsonIgnore, NJsonIgnore]
        public bool HasBookmarks => bookmarks.Count > 0;

        private void OnBookmarksChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(HasBookmarks));

        public BookmarkSettings()
        {
            bookmarks.CollectionChanged += OnBookmarksChanged;

            JumpCommand = new ActionCommand(
                _ => true,
                parameter =>
                {
                    if (parameter is BookmarkEntry entry)
                        BookmarkCommand.JumpTo(entry.Frame);
                });

            RemoveCommand = new ActionCommand(
                _ => true,
                parameter =>
                {
                    if (parameter is BookmarkEntry entry)
                        Bookmarks.Remove(entry);
                });
        }

        [JsonIgnore, NJsonIgnore]
        public ICommand JumpCommand { get; }

        [JsonIgnore, NJsonIgnore]
        public ICommand RemoveCommand { get; }

        [JsonIgnore, NJsonIgnore]
        public ICommand? ToggleCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("BookmarkToggleCommand");

        [JsonIgnore, NJsonIgnore]
        public ICommand? ClearCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("BookmarkClearCommand");

        [JsonIgnore, NJsonIgnore]
        public override Size MinSize => new(3, 1);

        [JsonIgnore, NJsonIgnore]
        public override ItemCategory Category => ItemCategory.Timeline;

        [JsonIgnore, NJsonIgnore]
        public override string Name => "Bookmark";

        [JsonIgnore, NJsonIgnore]
        public override string IconPath => YukkuriMovieMaker.Resources.Icons.IconKeys.Bookmark;

        [JsonIgnore, NJsonIgnore]
        public override string DisplayName => Texts.BookmarkDisplayName;

        [JsonIgnore, NJsonIgnore]
        public override string Description => Texts.BookmarkDescription;

        [JsonIgnore, NJsonIgnore]
        public override ICommand? DefaultCommand =>
            ToolBoxViewModel.Instance?.Commands.GetValueOrDefault("BookmarkToggleCommand");
    }
}
