using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToolBox.Items.Sort.Views
{
    public partial class SortItem : UserControl
    {
        public static readonly DependencyProperty SortAscendingCommandProperty =
            DependencyProperty.Register(nameof(SortAscendingCommand), typeof(ICommand), typeof(SortItem), new PropertyMetadata(null));

        public static readonly DependencyProperty SortDescendingCommandProperty =
            DependencyProperty.Register(nameof(SortDescendingCommand), typeof(ICommand), typeof(SortItem), new PropertyMetadata(null));

        public ICommand? SortAscendingCommand
        {
            get => (ICommand?)GetValue(SortAscendingCommandProperty);
            set => SetValue(SortAscendingCommandProperty, value);
        }

        public ICommand? SortDescendingCommand
        {
            get => (ICommand?)GetValue(SortDescendingCommandProperty);
            set => SetValue(SortDescendingCommandProperty, value);
        }

        public SortItem()
        {
            InitializeComponent();
        }
    }
}
