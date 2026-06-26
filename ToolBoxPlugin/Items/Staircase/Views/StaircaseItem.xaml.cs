using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;

namespace ToolBox.Items.Staircase.Views
{
    public partial class StaircaseItem : UserControl
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
