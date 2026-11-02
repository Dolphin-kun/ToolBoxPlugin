using System.Windows;
using ToolBox.Commons;
using ToolBox.ViewModel;

namespace ToolBox.View
{
    internal sealed class ToolBoxSettingsWindow : Window
    {
        public ToolBoxSettingsWindow(Window? owner, ToolBoxSettingsViewModel viewModel)
        {
            Title = Texts.SettingsTitle;
            Content = new ToolBoxSettingsPanel { DataContext = viewModel };
            ShowInTaskbar = false;
            Width = 720;
            Height = 560;
            MinWidth = 420;
            MinHeight = 300;

            WindowTheme.Attach(this);

            if (owner is null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                return;
            }

            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
    }
}
