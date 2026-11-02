using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SampleAddon.Views
{
    public partial class SampleHelloItemView : UserControl
    {
        public static readonly DependencyProperty ShowMessageItemCommandProperty =
            DependencyProperty.Register(nameof(ShowMessageItemCommand), typeof(ICommand), typeof(SampleHelloItemView), new PropertyMetadata(null));

        public ICommand? ShowMessageItemCommand
        {
            get => (ICommand?)GetValue(ShowMessageItemCommandProperty);
            set => SetValue(ShowMessageItemCommandProperty, value);
        }

        public SampleHelloItemView()
        {
            InitializeComponent();
        }
    }
}
