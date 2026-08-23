using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.Bookmarks.Views
{
    public partial class BookmarkItem : ToolItemView
    {
        public static readonly DependencyProperty BookmarkToggleCommandProperty =
            DependencyProperty.Register(nameof(BookmarkToggleCommand), typeof(ICommand), typeof(BookmarkItem));

        public static readonly DependencyProperty BookmarkNextCommandProperty =
            DependencyProperty.Register(nameof(BookmarkNextCommand), typeof(ICommand), typeof(BookmarkItem));

        public static readonly DependencyProperty BookmarkPreviousCommandProperty =
            DependencyProperty.Register(nameof(BookmarkPreviousCommand), typeof(ICommand), typeof(BookmarkItem));

        public static readonly DependencyProperty BookmarkClearCommandProperty =
            DependencyProperty.Register(nameof(BookmarkClearCommand), typeof(ICommand), typeof(BookmarkItem));

        public ICommand? BookmarkToggleCommand
        {
            get => (ICommand?)GetValue(BookmarkToggleCommandProperty);
            set => SetValue(BookmarkToggleCommandProperty, value);
        }

        public ICommand? BookmarkNextCommand
        {
            get => (ICommand?)GetValue(BookmarkNextCommandProperty);
            set => SetValue(BookmarkNextCommandProperty, value);
        }

        public ICommand? BookmarkPreviousCommand
        {
            get => (ICommand?)GetValue(BookmarkPreviousCommandProperty);
            set => SetValue(BookmarkPreviousCommandProperty, value);
        }

        public ICommand? BookmarkClearCommand
        {
            get => (ICommand?)GetValue(BookmarkClearCommandProperty);
            set => SetValue(BookmarkClearCommandProperty, value);
        }

        public BookmarkItem() => InitializeComponent();
    }
}
