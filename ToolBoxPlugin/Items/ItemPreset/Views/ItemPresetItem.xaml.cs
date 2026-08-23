using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.ItemPreset.Views
{
    public partial class ItemPresetItem : ToolItemView
    {
        public static readonly DependencyProperty AddPresetItemCommandProperty =
            DependencyProperty.Register(nameof(AddPresetItemCommand), typeof(ICommand), typeof(ItemPresetItem));

        public ICommand? AddPresetItemCommand
        {
            get => (ICommand?)GetValue(AddPresetItemCommandProperty);
            set => SetValue(AddPresetItemCommandProperty, value);
        }

        public ItemPresetItem() => InitializeComponent();
    }
}
