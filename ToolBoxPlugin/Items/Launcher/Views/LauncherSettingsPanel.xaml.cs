using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ToolBox.Items.Launcher.Settings;

namespace ToolBox.Items.Launcher.Views
{
    public partial class LauncherSettingsPanel : UserControl
    {
        public LauncherSettingsPanel()
        {
            InitializeComponent();
        }

        private void OnBrowseFolderClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not LauncherSettings settings) return;

            var dialog = new OpenFolderDialog
            {
                Title = "開くフォルダーを選択",
                InitialDirectory = GetInitialDirectory(settings.TargetPath),
            };

            if (dialog.ShowDialog() == true)
                settings.TargetPath = dialog.FolderName;
        }

        private static string GetInitialDirectory(string? currentPath)
        {
            if (string.IsNullOrWhiteSpace(currentPath))
                return string.Empty;

            try
            {
                var expanded = Environment.ExpandEnvironmentVariables(currentPath);
                if (Directory.Exists(expanded))
                    return expanded;

                var parent = Path.GetDirectoryName(expanded);
                return Directory.Exists(parent) ? parent! : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
