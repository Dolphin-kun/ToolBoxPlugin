using ToolBox.Commons;
using ToolBox.Items.Bookmarks.Settings;
using ToolBox.Items.Bookmarks.Views;

namespace ToolBox.Items.Bookmarks
{
    public sealed class Bookmark : DraggableItem
    {
        public override Type SettingsType => typeof(BookmarkSettings);
        public override Type ViewType => typeof(BookmarkItem);
        public override Type? ViewModelType => null;
        public override Type? SettingsViewType => typeof(BookmarkSettingsPanel);
        public override Type? SettingsViewModelType => null;

        public override IReadOnlyList<ToolItemCommandDefinition> GetCommandDefinitions() =>
        [
            new("BookmarkToggleCommand",
                BookmarkCommand.CanExecute,
                BookmarkCommand.Toggle,
                displayName: Texts.BookmarkToggleCommandName),
            new("BookmarkNextCommand",
                BookmarkCommand.HasBookmarks,
                BookmarkCommand.MoveToNext,
                displayName: Texts.BookmarkNextCommandName),
            new("BookmarkPreviousCommand",
                BookmarkCommand.HasBookmarks,
                BookmarkCommand.MoveToPrevious,
                displayName: Texts.BookmarkPreviousCommandName),
            new("BookmarkClearCommand",
                BookmarkCommand.HasBookmarks,
                BookmarkCommand.Clear,
                displayName: Texts.BookmarkClearCommandName),
        ];
    }
}
