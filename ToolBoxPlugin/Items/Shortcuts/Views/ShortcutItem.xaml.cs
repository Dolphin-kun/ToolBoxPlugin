using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.Shortcuts.Views
{
    public partial class ShortcutItem : ToolItemView
    {
        public static readonly DependencyProperty ShortcutCommandProperty =
            DependencyProperty.Register(nameof(ShortcutCommand), typeof(ICommand), typeof(ShortcutItem));

        public ICommand? ShortcutCommand
        {
            get => (ICommand?)GetValue(ShortcutCommandProperty);
            set => SetValue(ShortcutCommandProperty, value);
        }

        public ShortcutItem() => InitializeComponent();
    }
}
