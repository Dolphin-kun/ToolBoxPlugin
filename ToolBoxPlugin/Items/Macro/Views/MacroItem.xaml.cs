using System.Windows;
using System.Windows.Input;
using ToolBox.Controls;

namespace ToolBox.Items.Macro.Views
{
    public partial class MacroItem : ToolItemView
    {
        public static readonly DependencyProperty RunMacroItemCommandProperty =
            DependencyProperty.Register(nameof(RunMacroItemCommand), typeof(ICommand), typeof(MacroItem));

        public ICommand? RunMacroItemCommand
        {
            get => (ICommand?)GetValue(RunMacroItemCommandProperty);
            set => SetValue(RunMacroItemCommandProperty, value);
        }

        public MacroItem() => InitializeComponent();
    }
}
