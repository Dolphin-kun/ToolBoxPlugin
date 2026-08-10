using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToolBox.Commons
{
    public static class AddonMarketplace
    {
        public const string Topic = "ymm4-toolbox-addon";

        private const string SearchUrl =
            "https://api.github.com/search/repositories?q=topic:" + Topic + "&sort=stars&order=desc&per_page=50";

        private static readonly Lazy<HttpClient> Client = new(() =>
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YMM4-ToolBoxPlugin", "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            return client;
        });

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static async Task<IReadOnlyList<MarketplaceAddon>> SearchAsync(CancellationToken cancellationToken = default)
        {
            var json = await Client.Value.GetStringAsync(SearchUrl, cancellationToken).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<SearchResult>(json, JsonOptions);

            if (result?.Items == null)
                return [];

            return [.. result.Items.Select(repo => new MarketplaceAddon(
                repo.FullName ?? repo.Name ?? "(unknown)",
                repo.Name ?? "(unknown)",
                repo.Owner?.Login ?? string.Empty,
                repo.Description ?? string.Empty,
                repo.HtmlUrl ?? string.Empty,
                repo.StargazersCount,
                repo.PushedAt))];
        }

        public static async Task<IReadOnlyList<string>> InstallAsync(MarketplaceAddon addon, CancellationToken cancellationToken = default)
        {
            var releaseUrl = $"https://api.github.com/repos/{addon.FullName}/releases/latest";
            var json = await Client.Value.GetStringAsync(releaseUrl, cancellationToken).ConfigureAwait(false);
            var release = JsonSerializer.Deserialize<Release>(json, JsonOptions)
                ?? throw new InvalidOperationException("リリース情報を読み取れませんでした。");

            var asset = SelectAsset(release)
                ?? throw new InvalidOperationException("最新リリースに .zip も .dll も添付されていません。");

            var directory = AddonLoader.AddonDirectory;
            Directory.CreateDirectory(directory);

            var bytes = await Client.Value
                .GetByteArrayAsync(asset.BrowserDownloadUrl, cancellationToken)
                .ConfigureAwait(false);

            var installed = asset.Name!.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? ExtractDllsFromZip(bytes, directory)
                : WriteSingleDll(bytes, directory, asset.Name);

            if (installed.Count == 0)
                throw new InvalidOperationException("ダウンロードした成果物に DLL が含まれていませんでした。");

            AddonLoader.LoadAddons();
            return installed;
        }

        private static ReleaseAsset? SelectAsset(Release release)
        {
            var assets = release.Assets ?? [];
            return assets.FirstOrDefault(a => a.Name?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true)
                ?? assets.FirstOrDefault(a => a.Name?.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) == true);
        }

        private static List<string> ExtractDllsFromZip(byte[] bytes, string directory)
        {
            var installed = new List<string>();

            using var stream = new MemoryStream(bytes);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                if (!entry.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    continue;

                var destination = Path.Combine(directory, entry.Name);
                entry.ExtractToFile(destination, overwrite: true);
                installed.Add(entry.Name);
            }

            return installed;
        }

        private static List<string> WriteSingleDll(byte[] bytes, string directory, string fileName)
        {
            var destination = Path.Combine(directory, Path.GetFileName(fileName));
            File.WriteAllBytes(destination, bytes);
            return [Path.GetFileName(fileName)];
        }

        #region GitHub API のレスポンス

        private sealed class SearchResult
        {
            public List<Repository>? Items { get; set; }
        }

        private sealed class Repository
        {
            public string? Name { get; set; }

            [JsonPropertyName("full_name")]
            public string? FullName { get; set; }

            public string? Description { get; set; }

            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }

            [JsonPropertyName("stargazers_count")]
            public int StargazersCount { get; set; }

            [JsonPropertyName("pushed_at")]
            public DateTimeOffset? PushedAt { get; set; }

            public RepositoryOwner? Owner { get; set; }
        }

        private sealed class RepositoryOwner
        {
            public string? Login { get; set; }
        }

        private sealed class Release
        {
            [JsonPropertyName("tag_name")]
            public string? TagName { get; set; }

            public List<ReleaseAsset>? Assets { get; set; }
        }

        private sealed class ReleaseAsset
        {
            public string? Name { get; set; }

            [JsonPropertyName("browser_download_url")]
            public string? BrowserDownloadUrl { get; set; }
        }

        #endregion
    }

    public record MarketplaceAddon(
        string FullName,
        string Name,
        string Owner,
        string Description,
        string HtmlUrl,
        int Stars,
        DateTimeOffset? PushedAt)
    {
        public string UpdatedText => PushedAt.HasValue
            ? $"最終更新 {PushedAt.Value.LocalDateTime:yyyy/MM/dd}"
            : string.Empty;

        public string StarsText => $"★ {Stars}";
    }
}
