using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolBox.Commons;
using ToolBox.Items.Macro.Settings;

namespace ToolBox.Items.Macro.Views
{
    public partial class MacroItem : UserControl
    {
        public static readonly DependencyProperty RunMacroItemCommandProperty =
            DependencyProperty.Register(nameof(RunMacroItemCommand), typeof(ICommand), typeof(MacroItem));

        public ICommand? RunMacroItemCommand
        {
            get => (ICommand?)GetValue(RunMacroItemCommandProperty);
            set => SetValue(RunMacroItemCommandProperty, value);
        }

        private readonly AdornerUtil adornerUtil = new();

        public MacroItem()
        {
            InitializeComponent();
            MouseRightButtonDown += OnMouseRightButtonDown;
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not MacroSettings settings) return;

            e.Handled = true;
            adornerUtil.Toggle(this, () => new MacroSettingsPanel { DataContext = settings });
        }
    }
}
