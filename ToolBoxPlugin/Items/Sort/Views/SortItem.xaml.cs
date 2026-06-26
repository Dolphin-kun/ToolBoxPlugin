using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToolBox.Items.Sort.Views
{
    public partial class SortItem : UserControl
    {
        public static readonly DependencyProperty SortAscendingItemCommandProperty =
            DependencyProperty.Register(nameof(SortAscendingItemCommand), typeof(ICommand), typeof(SortItem), new PropertyMetadata(null));

        public static readonly DependencyProperty SortDescendingItemCommandProperty =
            DependencyProperty.Register(nameof(SortDescendingItemCommand), typeof(ICommand), typeof(SortItem), new PropertyMetadata(null));

        public ICommand? SortAscendingItemCommand
        {
            get => (ICommand?)GetValue(SortAscendingItemCommandProperty);
            set => SetValue(SortAscendingItemCommandProperty, value);
        }

        public ICommand? SortDescendingItemCommand
        {
            get => (ICommand?)GetValue(SortDescendingItemCommandProperty);
            set => SetValue(SortDescendingItemCommandProperty, value);
        }

        public SortItem()
        {
            InitializeComponent();
        }
    }
}
