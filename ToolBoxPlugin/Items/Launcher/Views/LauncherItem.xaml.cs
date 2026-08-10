using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Launcher.Settings;

namespace ToolBox.Items.Launcher.Views
{
    public partial class LauncherItem : UserControl
    {
        public static readonly DependencyProperty LaunchItemCommandProperty =
            DependencyProperty.Register(nameof(LaunchItemCommand), typeof(ICommand), typeof(LauncherItem));

        public ICommand? LaunchItemCommand
        {
            get => (ICommand?)GetValue(LaunchItemCommandProperty);
            set => SetValue(LaunchItemCommandProperty, value);
        }

        private readonly AdornerUtil adornerUtil = new();

        public LauncherItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not LauncherSettings settings) return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new LauncherSettingsPanel { DataContext = settings });
        }
    }
}
