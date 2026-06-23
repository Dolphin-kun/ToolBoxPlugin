using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToolBox.Items.Audio.Views
{
    public partial class AudioItem : UserControl
    {
        public static readonly DependencyProperty DetachAudioFromVideoItemCommandProperty =
            DependencyProperty.Register(nameof(DetachAudioFromVideoItemCommand), typeof(ICommand), typeof(AudioItem));

        public ICommand? DetachAudioFromVideoItemCommand
        {
            get => (ICommand?)GetValue(DetachAudioFromVideoItemCommandProperty);
            set => SetValue(DetachAudioFromVideoItemCommandProperty, value);
        }

        public AudioItem() => InitializeComponent();
    }
}
