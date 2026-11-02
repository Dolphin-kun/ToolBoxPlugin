using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ToolBox.Controls;

namespace ToolBox.Items.InsertIntoGroup.Views
{
    public partial class InsertIntoGroupItem : ToolItemView
    {
        public static readonly DependencyProperty InsertIntoGroupItemCommandProperty =
            DependencyProperty.Register(nameof(InsertIntoGroupItemCommand), typeof(ICommand), typeof(InsertIntoGroupItem));

        public ICommand? InsertIntoGroupItemCommand
        {
            get => (ICommand?)GetValue(InsertIntoGroupItemCommandProperty);
            set => SetValue(InsertIntoGroupItemCommandProperty, value);
        }

        public InsertIntoGroupItem() => InitializeComponent();
    }
}
