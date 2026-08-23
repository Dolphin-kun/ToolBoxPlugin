using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.GapCheck.Views
{
    public partial class GapCheckItem : ToolItemView
    {
        public static readonly DependencyProperty GapCheckNextCommandProperty =
            DependencyProperty.Register(nameof(GapCheckNextCommand), typeof(ICommand), typeof(GapCheckItem));

        public static readonly DependencyProperty GapCheckPreviousCommandProperty =
            DependencyProperty.Register(nameof(GapCheckPreviousCommand), typeof(ICommand), typeof(GapCheckItem));

        public static readonly DependencyProperty GapCheckSummaryCommandProperty =
            DependencyProperty.Register(nameof(GapCheckSummaryCommand), typeof(ICommand), typeof(GapCheckItem));

        public ICommand? GapCheckNextCommand
        {
            get => (ICommand?)GetValue(GapCheckNextCommandProperty);
            set => SetValue(GapCheckNextCommandProperty, value);
        }

        public ICommand? GapCheckPreviousCommand
        {
            get => (ICommand?)GetValue(GapCheckPreviousCommandProperty);
            set => SetValue(GapCheckPreviousCommandProperty, value);
        }

        public ICommand? GapCheckSummaryCommand
        {
            get => (ICommand?)GetValue(GapCheckSummaryCommandProperty);
            set => SetValue(GapCheckSummaryCommandProperty, value);
        }

        public GapCheckItem() => InitializeComponent();
    }
}
