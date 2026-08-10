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

        /// <summary>
        /// このプラグイン本体のリポジトリ。トピックは付いているがアドオンではないので一覧から外す。
        /// </summary>
        private const string SelfRepository = "Dolphin-kun/ToolBoxPlugin";

        /// <summary>1 回の問い合わせで取れる最大件数 (GitHub の上限)。</summary>
        private const int PageSize = 100;

        /// <summary>
        /// 検索 API は合計 1000 件までしか辿れない。
        /// 1 ページ 100 件なので、10 ページで打ち止めになる。
        /// </summary>
        private const int MaxPages = 10;

        private static string BuildSearchUrl(int page) =>
            $"https://api.github.com/search/repositories?q=topic:{Topic}&sort=stars&order=desc&per_page={PageSize}&page={page}";

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

        private static SearchResponse? cachedResponse;
        private static DateTimeOffset cacheExpiresAt;

        /// <summary>
        /// 検索 API は認証なしだと 1 分あたり 10 回までなので、
        /// 一度取得した一覧はしばらく使い回して無駄な問い合わせを避ける。
        /// </summary>
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(10);

        /// <summary>
        /// トピックの付いたリポジトリをすべて取得する。
        /// 1 ページ 100 件で、まだ残りがあれば次のページも取りに行く。
        /// </summary>
        public static async Task<SearchResponse> SearchAsync(CancellationToken cancellationToken = default)
        {
            if (cachedResponse != null && DateTimeOffset.UtcNow < cacheExpiresAt)
                return cachedResponse with { IsFromCache = true, CacheExpiresAt = cacheExpiresAt };

            var addons = new List<MarketplaceAddon>();
            var totalCount = 0;
            var fetchedCount = 0;
            var excludedCount = 0;

            for (var page = 1; page <= MaxPages; page++)
            {
                var json = await GetStringAsync(BuildSearchUrl(page), cancellationToken).ConfigureAwait(false);
                var result = JsonSerializer.Deserialize<SearchResult>(json, JsonOptions);

                if (result?.Items == null || result.Items.Count == 0)
                    break;

                totalCount = result.TotalCount;
                fetchedCount += result.Items.Count;

                foreach (var repo in result.Items)
                {
                    if (IsSelf(repo))
                    {
                        excludedCount++;
                        continue;
                    }

                    addons.Add(ToAddon(repo));
                }

                // 一覧から外した分も取得済みなので、取得件数で残りを判断する
                if (fetchedCount >= totalCount)
                    break;
            }

            cachedResponse = new SearchResponse(
                addons,
                Math.Max(addons.Count, totalCount - excludedCount),
                IsFromCache: false,
                DateTimeOffset.MinValue);
            cacheExpiresAt = DateTimeOffset.UtcNow + CacheLifetime;

            return cachedResponse with { CacheExpiresAt = cacheExpiresAt };
        }

        private static bool IsSelf(Repository repo) =>
            string.Equals(repo.FullName, SelfRepository, StringComparison.OrdinalIgnoreCase);

        private static MarketplaceAddon ToAddon(Repository repo) => new(
            repo.FullName ?? repo.Name ?? "(unknown)",
            repo.Name ?? "(unknown)",
            repo.Owner?.Login ?? string.Empty,
            repo.Description ?? string.Empty,
            repo.HtmlUrl ?? string.Empty,
            repo.StargazersCount,
            repo.PushedAt);

        /// <summary>
        /// 回数制限に当たったときは HTTP 403 と共に X-RateLimit-Remaining: 0 が返る。
        /// そのまま例外を投げると原因が分からないので、いつ回復するかを添えて伝える。
        /// </summary>
        private static async Task<string> GetStringAsync(string url, CancellationToken cancellationToken)
        {
            using var response = await Client.Value.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (IsRateLimited(response))
                throw new InvalidOperationException(BuildRateLimitMessage(response));

            throw new InvalidOperationException($"GitHub から応答がありませんでした ({(int)response.StatusCode})。");
        }

        private static bool IsRateLimited(HttpResponseMessage response)
        {
            if (response.StatusCode is not (System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.TooManyRequests))
                return false;

            return !response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining)
                || remaining.FirstOrDefault() == "0";
        }

        private static string BuildRateLimitMessage(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("X-RateLimit-Reset", out var reset)
                && long.TryParse(reset.FirstOrDefault(), out var epochSeconds))
            {
                var resetAt = DateTimeOffset.FromUnixTimeSeconds(epochSeconds).LocalDateTime;
                return $"GitHub の問い合わせ回数の上限に達しました。{resetAt:HH:mm} 以降にもう一度お試しください。";
            }

            return "GitHub の問い合わせ回数の上限に達しました。しばらく待ってからもう一度お試しください。";
        }

        public static async Task<IReadOnlyList<string>> InstallAsync(MarketplaceAddon addon, CancellationToken cancellationToken = default)
        {
            var releaseUrl = $"https://api.github.com/repos/{addon.FullName}/releases/latest";
            var json = await GetStringAsync(releaseUrl, cancellationToken).ConfigureAwait(false);
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
            [JsonPropertyName("total_count")]
            public int TotalCount { get; set; }

            public List<Repository>? Items { get; set; }
        }

        public sealed class Repository
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

        public sealed class RepositoryOwner
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

    /// <param name="TotalCount">GitHub 側の総件数。取得できた件数より多い場合は 1000 件の上限に当たっている。</param>
    public record SearchResponse(
        IReadOnlyList<MarketplaceAddon> Addons,
        int TotalCount,
        bool IsFromCache,
        DateTimeOffset CacheExpiresAt)
    {
        public bool IsTruncated => Addons.Count < TotalCount;
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
