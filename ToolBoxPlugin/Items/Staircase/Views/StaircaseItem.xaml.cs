using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.Staircase.Views
{
    public partial class StaircaseItem : ToolItemView
    {
        public static readonly DependencyProperty StaircaseItemCommandProperty =
            DependencyProperty.Register(nameof(StaircaseItemCommand), typeof(ICommand), typeof(StaircaseItem));

        public static readonly DependencyProperty ReverseStaircaseItemCommandProperty =
            DependencyProperty.Register(nameof(ReverseStaircaseItemCommand), typeof(ICommand), typeof(StaircaseItem));

        public ICommand? StaircaseItemCommand
        {
            get => (ICommand?)GetValue(StaircaseItemCommandProperty);
            set => SetValue(StaircaseItemCommandProperty, value);
        }

        public ICommand? ReverseStaircaseItemCommand
        {
            get => (ICommand?)GetValue(ReverseStaircaseItemCommandProperty);
            set => SetValue(ReverseStaircaseItemCommandProperty, value);
        }

        public StaircaseItem() => InitializeComponent();
    }
}
