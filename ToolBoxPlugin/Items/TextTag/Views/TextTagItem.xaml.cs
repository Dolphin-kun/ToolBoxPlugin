using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ToolBox.Controls;

namespace ToolBox.Items.TextTag.Views
{
    public partial class TextTagItem : ToolItemView
    {
        public static readonly DependencyProperty TextTagCommandProperty =
            DependencyProperty.Register(nameof(TextTagCommand), typeof(ICommand), typeof(TextTagItem));

        public ICommand? TextTagCommand
        {
            get => (ICommand?)GetValue(TextTagCommandProperty);
            set => SetValue(TextTagCommandProperty, value);
        }

        public TextTagItem() => InitializeComponent();
    }
}
