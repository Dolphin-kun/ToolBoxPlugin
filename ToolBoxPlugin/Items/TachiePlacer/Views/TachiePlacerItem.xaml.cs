using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.TachiePlacer.Settings;

namespace ToolBox.Items.TachiePlacer.Views
{
    public partial class TachiePlacerItem : UserControl
    {
        public static readonly DependencyProperty TachiePlacerItemCommandProperty =
            DependencyProperty.Register(nameof(TachiePlacerItemCommand), typeof(ICommand), typeof(TachiePlacerItem));

        public ICommand? TachiePlacerItemCommand
        {
            get => (ICommand?)GetValue(TachiePlacerItemCommandProperty);
            set => SetValue(TachiePlacerItemCommandProperty, value);
        }

        private readonly AdornerUtil adornerUtil = new();

        public TachiePlacerItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not TachiePlacerSettings settings) return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new TachiePlacerSettingsPanel { DataContext = settings });
        }
    }
}
