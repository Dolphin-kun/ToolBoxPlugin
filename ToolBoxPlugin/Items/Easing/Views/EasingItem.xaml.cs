using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.Easing.Views
{
    public partial class EasingItem : ToolItemView
    {
        public static readonly DependencyProperty ApplyEasingCommandProperty =
            DependencyProperty.Register(nameof(ApplyEasingCommand), typeof(ICommand), typeof(EasingItem));

        public ICommand? ApplyEasingCommand
        {
            get => (ICommand?)GetValue(ApplyEasingCommandProperty);
            set => SetValue(ApplyEasingCommandProperty, value);
        }

        public EasingItem() => InitializeComponent();
    }
}
