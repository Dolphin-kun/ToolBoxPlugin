using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.TextSplitter.Views
{
    public partial class TextSplitterItem : ToolItemView
    {
        public static readonly DependencyProperty TextSplitterItemCommandProperty =
            DependencyProperty.Register(nameof(TextSplitterItemCommand), typeof(ICommand), typeof(TextSplitterItem));

        public ICommand? TextSplitterItemCommand
        {
            get => (ICommand?)GetValue(TextSplitterItemCommandProperty);
            set => SetValue(TextSplitterItemCommandProperty, value);
        }

        public TextSplitterItem() => InitializeComponent();
    }
}
