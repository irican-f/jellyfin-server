using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using MediaBrowser.Providers.Plugins.AniList.Configuration;
using Microsoft.Extensions.Logging;

// API v2
namespace MediaBrowser.Providers.Plugins.AniList.Providers.AniList
{
    /// <summary>
    /// AniList series provider.
    /// </summary>
    [SuppressMessage("Globalization", "CA1305:Spécifier IFormatProvider", Justification = "Not my code")]
    public class AniListSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly IApplicationPaths _paths;
        private readonly ILogger<AniListSeriesProvider> _log;
        private readonly AniListApi _aniListApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="AniListSeriesProvider"/> class.
        /// </summary>
        /// <param name="appPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
        public AniListSeriesProvider(IApplicationPaths appPaths, ILogger<AniListSeriesProvider> logger)
        {
            _log = logger;
            _aniListApi = new AniListApi();
            _paths = appPaths;
        }

        /// <inheritdoc />
        public int Order => -2;

        /// <inheritdoc />
        public string Name => "AniList";

        /// <inheritdoc />
        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>();
            Media? media = null;
            PluginConfiguration config = Plugin.Instance?.Configuration!;

            var aid = info.ProviderIds.GetOrDefault(ProviderNames.AniList);
            if (!string.IsNullOrEmpty(aid))
            {
                media = await _aniListApi.GetAnime(aid).ConfigureAwait(false);
            }
            else
            {
                var searchName = info.Name;
                MediaSearchResult? msr;
                if (config.UseAnitomyLibrary)
                {
                    // Use Anitomy to extract the title
                    var processedSearchName = AnitomyHelper.ExtractAnimeTitle(searchName) ?? searchName;
                    // TODO Add episode/season?
                    searchName = AnilistSearchHelper.PreprocessTitle(processedSearchName);
                    _log.LogInformation("Start AniList... Searching({Name})", searchName);
                    msr = await _aniListApi.SearchGetSeries(searchName, cancellationToken).ConfigureAwait(false);

                    if (msr != null)
                    {
                        media = await _aniListApi.GetAnime(msr.Id.ToString()).ConfigureAwait(false);
                    }
                }

                if (!config.UseAnitomyLibrary || media == null)
                {
                    searchName = info.Name;
                    searchName = AnilistSearchHelper.PreprocessTitle(searchName);
                    _log.LogInformation("Start AniList... Searching({Name})", searchName);
                    msr = await _aniListApi.SearchGetSeries(info.Name, cancellationToken).ConfigureAwait(false);

                    if (msr != null)
                    {
                        media = await _aniListApi.GetAnime(msr.Id.ToString()).ConfigureAwait(false);
                    }
                }
            }

            if (media != null)
            {
                result.HasMetadata = true;
                result.Item = media.ToSeries();
                result.People = media.GetPeopleInfo();
                result.Provider = ProviderNames.AniList;
                StoreImageUrl(media.Id.ToString(), media.GetImageUrl(), "image");
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();

            var aid = searchInfo.ProviderIds.GetOrDefault(ProviderNames.AniList);
            if (!string.IsNullOrEmpty(aid))
            {
                Media? aidResult = await _aniListApi.GetAnime(aid).ConfigureAwait(false);
                if (aidResult != null)
                {
                    results.Add(aidResult.ToSearchResult());
                }
            }

            if (!string.IsNullOrEmpty(searchInfo.Name))
            {
                List<MediaSearchResult> nameResults = await _aniListApi.SearchGetSeriesList(searchInfo.Name, cancellationToken).ConfigureAwait(false) ?? new List<MediaSearchResult>();
                results.AddRange(nameResults.Select(media => media.ToSearchResult()));
            }

            return results;
        }

        private void StoreImageUrl(string series, string? url, string type)
        {
            var path = Path.Combine(_paths.CachePath, "anilist", type, series + ".txt");
            var directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);

            File.WriteAllText(path, url);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            var httpClient = Plugin.Instance?.GetHttpClient()!;

            return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }
}
