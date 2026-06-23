using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;

namespace ToolBox.Items.Staircase.Views
{
    public partial class StaircaseItem : UserControl
    {
        public static readonly DependencyProperty StaircaseCommandProperty =
            DependencyProperty.Register(nameof(StaircaseCommand), typeof(ICommand), typeof(StaircaseItem));

        public static readonly DependencyProperty ReverseStaircaseCommandProperty =
            DependencyProperty.Register(nameof(ReverseStaircaseCommand), typeof(ICommand), typeof(StaircaseItem));

        public ICommand? StaircaseCommand
        {
            get => (ICommand?)GetValue(StaircaseCommandProperty);
            set => SetValue(StaircaseCommandProperty, value);
        }

        public ICommand? ReverseStaircaseCommand
        {
            get => (ICommand?)GetValue(ReverseStaircaseCommandProperty);
            set => SetValue(ReverseStaircaseCommandProperty, value);
        }

        private readonly AdornerUtil adornerUtil = new();

        public StaircaseItem() => InitializeComponent();

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            e.Handled = true;

            adornerUtil.Toggle(this, () => new StaircaseSettingsPanel { DataContext = DataContext });
        }
    }
}
