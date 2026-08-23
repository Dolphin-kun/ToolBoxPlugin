using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.Launcher.Views
{
    public partial class LauncherItem : ToolItemView
    {
        public static readonly DependencyProperty LaunchItemCommandProperty =
            DependencyProperty.Register(nameof(LaunchItemCommand), typeof(ICommand), typeof(LauncherItem));

        public ICommand? LaunchItemCommand
        {
            get => (ICommand?)GetValue(LaunchItemCommandProperty);
            set => SetValue(LaunchItemCommandProperty, value);
        }

        public LauncherItem() => InitializeComponent();
    }
}
