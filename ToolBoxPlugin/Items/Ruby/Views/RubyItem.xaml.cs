using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Ruby.Settings;

namespace ToolBox.Items.Ruby.Views
{
    public partial class RubyItem : UserControl
    {
        public static readonly DependencyProperty RubyItemCommandProperty =
            DependencyProperty.Register(nameof(RubyItemCommand), typeof(ICommand), typeof(RubyItem));

        public ICommand? RubyItemCommand
        {
            get => (ICommand?)GetValue(RubyItemCommandProperty);
            set => SetValue(RubyItemCommandProperty, value);
        }

        private readonly AdornerUtil adornerUtil = new();

        public RubyItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not RubySettings settings) return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new RubySettingsPanel { DataContext = settings });
        }
    }
}
