using System.Diagnostics;
using System.IO;
using System.Windows;
using ToolBox.Items.Launcher.Settings;

namespace ToolBox.Items.Launcher
{
    public static class LauncherCommand
    {
        public static bool CanLaunch(LauncherSettings settings) =>
            !string.IsNullOrWhiteSpace(settings.TargetPath);

        public static void Launch(LauncherSettings settings)
        {
            var target = settings.TargetPath?.Trim();
            if (string.IsNullOrWhiteSpace(target))
                return;

            target = Environment.ExpandEnvironmentVariables(target);

            if (!IsUrl(target) && !File.Exists(target) && !Directory.Exists(target))
            {
                MessageBox.Show(
                    $"開けませんでした。次の場所が見つかりません。\n\n{target}",
                    "ファイル・フォルダーを開く",
                    MessageBoxButton.OK);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true,
                WorkingDirectory = File.Exists(target) ? Path.GetDirectoryName(target) ?? string.Empty : string.Empty,
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"開けませんでした。\n\n{target}\n\n{ex.Message}",
                    "ファイル・フォルダーを開く",
                    MessageBoxButton.OK);
            }
        }

        private static bool IsUrl(string target) =>
            Uri.TryCreate(target, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
