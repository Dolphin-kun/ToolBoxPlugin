using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Shortcuts.Settings;

namespace ToolBox.Items.Shortcuts.Views
{
    public partial class ShortcutItem : UserControl
    {
        public static readonly DependencyProperty ShortcutCommandProperty =
            DependencyProperty.Register(nameof(ShortcutCommand), typeof(ICommand), typeof(ShortcutItem));

        public ICommand? ShortcutCommand
        {
            get => (ICommand?)GetValue(ShortcutCommandProperty);
            set => SetValue(ShortcutCommandProperty, value);
        }

        private readonly AdornerUtil adornerUtil = new();

        public ShortcutItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not ShortcutSettings settings) return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new ShortcutSettingsPanel { DataContext = settings });
        }
    }
}
