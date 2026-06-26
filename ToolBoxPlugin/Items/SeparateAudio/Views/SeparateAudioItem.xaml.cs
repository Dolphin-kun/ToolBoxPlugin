using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToolBox.Items.SeparateAudio.Views
{
    public partial class SeparateAudioItem : UserControl
    {
        public static readonly DependencyProperty SeparateAudioItemCommandProperty =
            DependencyProperty.Register(nameof(SeparateAudioItemCommand), typeof(ICommand), typeof(SeparateAudioItem));

        public ICommand? SeparateAudioItemCommand
        {
            get => (ICommand?)GetValue(SeparateAudioItemCommandProperty);
            set => SetValue(SeparateAudioItemCommandProperty, value);
        }

        public SeparateAudioItem() => InitializeComponent();
    }
}
