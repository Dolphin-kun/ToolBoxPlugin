using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.TachiePlacer.Settings;

namespace ToolBox.Items.TachiePlacer.Views
{
    public partial class TachiePlacerItem : UserControl
    {
        public static readonly DependencyProperty AddTachieItemCommandProperty =
            DependencyProperty.Register(nameof(AddTachieItemCommand), typeof(ICommand), typeof(TachiePlacerItem));

        public ICommand? AddTachieItemCommand
        {
            get => (ICommand?)GetValue(AddTachieItemCommandProperty);
            set => SetValue(AddTachieItemCommandProperty, value);
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
