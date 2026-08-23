using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.TachiePlacer.Views
{
    public partial class TachiePlacerItem : ToolItemView
    {
        public static readonly DependencyProperty TachiePlacerItemCommandProperty =
            DependencyProperty.Register(nameof(TachiePlacerItemCommand), typeof(ICommand), typeof(TachiePlacerItem));

        public ICommand? TachiePlacerItemCommand
        {
            get => (ICommand?)GetValue(TachiePlacerItemCommandProperty);
            set => SetValue(TachiePlacerItemCommandProperty, value);
        }

        public TachiePlacerItem() => InitializeComponent();
    }
}
