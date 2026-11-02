using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ToolBox.Controls;

namespace ToolBox.Items.LayerEditor.Views
{
    public partial class LayerEditorItem : ToolItemView
    {
        public static readonly DependencyProperty LayerEditorApplyCommandProperty =
            DependencyProperty.Register(nameof(LayerEditorApplyCommand), typeof(ICommand), typeof(LayerEditorItem));

        public static readonly DependencyProperty LayerEditorResetCommandProperty =
            DependencyProperty.Register(nameof(LayerEditorResetCommand), typeof(ICommand), typeof(LayerEditorItem));

        public ICommand? LayerEditorApplyCommand
        {
            get => (ICommand?)GetValue(LayerEditorApplyCommandProperty);
            set => SetValue(LayerEditorApplyCommandProperty, value);
        }

        public ICommand? LayerEditorResetCommand
        {
            get => (ICommand?)GetValue(LayerEditorResetCommandProperty);
            set => SetValue(LayerEditorResetCommandProperty, value);
        }

        public LayerEditorItem() => InitializeComponent();
    }
}
