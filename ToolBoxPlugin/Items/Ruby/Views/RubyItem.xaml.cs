using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.Ruby.Views
{
    public partial class RubyItem : ToolItemView
    {
        public static readonly DependencyProperty RubyItemCommandProperty =
            DependencyProperty.Register(nameof(RubyItemCommand), typeof(ICommand), typeof(RubyItem));

        public ICommand? RubyItemCommand
        {
            get => (ICommand?)GetValue(RubyItemCommandProperty);
            set => SetValue(RubyItemCommandProperty, value);
        }

        public RubyItem() => InitializeComponent();
    }
}
