using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToolBox.Items.Group.Views
{
    public partial class GroupItem : UserControl
    {
        public static readonly DependencyProperty InsertIntoGroupCommandProperty =
            DependencyProperty.Register(nameof(InsertIntoGroupCommand), typeof(ICommand), typeof(GroupItem));

        public ICommand? InsertIntoGroupCommand
        {
            get => (ICommand?)GetValue(InsertIntoGroupCommandProperty);
            set => SetValue(InsertIntoGroupCommandProperty, value);
        }

        public GroupItem() => InitializeComponent();
    }
}
