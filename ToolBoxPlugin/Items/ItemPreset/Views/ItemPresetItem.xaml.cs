using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.ItemPreset.Settings;

namespace ToolBox.Items.ItemPreset.Views
{
    public partial class ItemPresetItem : UserControl
    {
        public static readonly DependencyProperty AddPresetItemCommandProperty =
            DependencyProperty.Register(nameof(AddPresetItemCommand), typeof(ICommand), typeof(ItemPresetItem));

        public ICommand? AddPresetItemCommand
        {
            get => (ICommand?)GetValue(AddPresetItemCommandProperty);
            set => SetValue(AddPresetItemCommandProperty, value);
        }

        private readonly AdornerUtil adornerUtil = new();

        public ItemPresetItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not ItemPresetSettings settings)
                return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new ItemPresetSettingsPanel { DataContext = settings });
        }
    }
}
