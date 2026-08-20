using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Input;
using ToolBox.Settings;
using YukkuriMovieMaker.Commons;

namespace ToolBox.ViewModel
{
    public class BasicSettingsViewModel : SettingsNodeViewModel
    {
        private static readonly HttpClient httpClient = new();
        private const string PostId = "1782201263495393";
        private const string ApiUrl = $"https://ymm4-info.net/api/plugin/{PostId}/version";

        public ToolBoxSettings Settings { get; }

        public static string CurrentVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        private string latestVersion = "取得中...";
        public string LatestVersion
        {
            get => latestVersion;
            set => Set(ref latestVersion, value);
        }

        private bool hasUpdate = false;
        public bool HasUpdate
        {
            get => hasUpdate;
            set => Set(ref hasUpdate, value);
        }

        private string downloadUrl = "";

        public ICommand OpenUpdateUrlCommand { get; }

        public BasicSettingsViewModel(ToolBoxSettings settings) : base("基本設定")
        {
            Settings = settings;
            OpenUpdateUrlCommand = new ActionCommand(
                _ => HasUpdate,
                _ => {
                    var targetUrl = !string.IsNullOrEmpty(downloadUrl) ? downloadUrl : "https://ymm4-info.net/";
                    Process.Start(new ProcessStartInfo { FileName = targetUrl, UseShellExecute = true });
                }
            );

            _ = CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, ApiUrl);
                request.Headers.Add("x-ymm4-plugin-check", "true");

                var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("success", out var successElement) && successElement.GetBoolean() &&
                    root.TryGetProperty("version", out var versionElement) &&
                    root.TryGetProperty("downloadURL", out var urlElement))
                {
                    var apiVersionString = versionElement.GetString();
                    downloadUrl = urlElement.GetString() ?? "";

                    LatestVersion = apiVersionString ?? "不明";

                    var currentVersionString = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
                    var cleanApiVersionString = apiVersionString?.TrimStart('v', 'V');

                    if (Version.TryParse(cleanApiVersionString, out var apiVersion) &&
                        Version.TryParse(currentVersionString, out var currentVersion))
                    {
                        if (apiVersion > currentVersion)
                        {
                            HasUpdate = true;
                        }
                    }
                }
            }
            catch
            {
                LatestVersion = "取得失敗";
            }
        }
    }
}
