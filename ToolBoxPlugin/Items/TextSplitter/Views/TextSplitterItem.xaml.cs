using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.TextSplitter.Settings;

namespace ToolBox.Items.TextSplitter.Views
{
    public partial class TextSplitterItem : UserControl
    {
        public static readonly DependencyProperty TextSplitterItemCommandProperty =
            DependencyProperty.Register(nameof(TextSplitterItemCommand), typeof(ICommand), typeof(TextSplitterItem));

        public ICommand? TextSplitterItemCommand
        {
            get => (ICommand?)GetValue(TextSplitterItemCommandProperty);
            set => SetValue(TextSplitterItemCommandProperty, value);
        }

        private readonly AdornerUtil adornerUtil = new();

        public TextSplitterItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not TextSplitterSettings settings) return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new TextSplitterSettingsPanel { DataContext = settings });
        }
    }
}
