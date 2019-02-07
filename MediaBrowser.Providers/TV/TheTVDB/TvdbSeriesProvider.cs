using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using TvDbSharper;
using TvDbSharper.Dto;
using Series = MediaBrowser.Controller.Entities.TV.Series;

namespace MediaBrowser.Providers.TV.TheTVDB
{
    public class TvdbSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        internal static TvdbSeriesProvider Current { get; private set; }
        private readonly IHttpClient _httpClient;
        private readonly CultureInfo _usCulture = new CultureInfo("en-US");
        private readonly ILogger _logger;
        private readonly ILibraryManager _libraryManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly TvDbClientManager _tvDbClientManager;

        public TvdbSeriesProvider(IHttpClient httpClient, ILogger logger, ILibraryManager libraryManager, ILocalizationManager localizationManager)
        {
            _httpClient = httpClient;
            _logger = logger;
            _libraryManager = libraryManager;
            _localizationManager = localizationManager;
            Current = this;
            _tvDbClientManager = TvDbClientManager.Instance;
        }

        private string NormalizeLanguage(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return language;
            }

            // pt-br is just pt to tvdb
            return language.Split('-')[0].ToLowerInvariant();
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            if (IsValidSeries(searchInfo.ProviderIds))
            {
                var metadata = await GetMetadata(searchInfo, cancellationToken).ConfigureAwait(false);

                if (metadata.HasMetadata)
                {
                    return new List<RemoteSearchResult>
                    {
                        new RemoteSearchResult
                        {
                            Name = metadata.Item.Name,
                            PremiereDate = metadata.Item.PremiereDate,
                            ProductionYear = metadata.Item.ProductionYear,
                            ProviderIds = metadata.Item.ProviderIds,
                            SearchProviderName = Name
                        }
                    };
                }
            }

            return await FindSeries(searchInfo.Name, searchInfo.Year, searchInfo.MetadataLanguage, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo itemId, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>
            {
                QueriedById = true
            };

            if (!IsValidSeries(itemId.ProviderIds))
            {
                result.QueriedById = false;
                await Identify(itemId).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (IsValidSeries(itemId.ProviderIds))
            {
                result.Item = new Series();
                result.HasMetadata = true;

                await FetchSeriesData(result, itemId.MetadataLanguage, itemId.ProviderIds, cancellationToken);
            }

            return result;
        }

        private async Task FetchSeriesData(MetadataResult<Series> result, string metadataLanguage, Dictionary<string, string> seriesProviderIds, CancellationToken cancellationToken)
        {
            _tvDbClientManager.TvDbClient.AcceptedLanguage = NormalizeLanguage(metadataLanguage);
            var series = result.Item;

            if (seriesProviderIds.TryGetValue(MetadataProviders.Tvdb.ToString(), out var tvdbId) && !string.IsNullOrEmpty(tvdbId))
            {
                series.SetProviderId(MetadataProviders.Tvdb, tvdbId);
            }

            if (seriesProviderIds.TryGetValue(MetadataProviders.Imdb.ToString(), out var imdbId) && !string.IsNullOrEmpty(imdbId))
            {
                series.SetProviderId(MetadataProviders.Imdb, imdbId);
                tvdbId = await GetSeriesByRemoteId(imdbId, MetadataProviders.Imdb.ToString(), metadataLanguage, cancellationToken);
            }

            if (seriesProviderIds.TryGetValue(MetadataProviders.Zap2It.ToString(), out var zap2It) && !string.IsNullOrEmpty(zap2It))
            {
                series.SetProviderId(MetadataProviders.Zap2It, zap2It);
                tvdbId = await GetSeriesByRemoteId(zap2It, MetadataProviders.Zap2It.ToString(), metadataLanguage, cancellationToken);
            }

            var seriesResult = await _tvDbClientManager.GetSeriesByIdAsync(Convert.ToInt32(tvdbId), cancellationToken);

            MapSeriesToResult(result, seriesResult.Data);

            cancellationToken.ThrowIfCancellationRequested();

            result.ResetPeople();

            var actorsResult = await _tvDbClientManager.GetActorsAsync(Convert.ToInt32(tvdbId), cancellationToken);
            MapActorsToResult(result, actorsResult.Data);
        }

        private async Task<string> GetSeriesByRemoteId(string id, string idType, string language, CancellationToken cancellationToken)
        {
            _tvDbClientManager.TvDbClient.AcceptedLanguage = NormalizeLanguage(language);
            TvDbResponse<SeriesSearchResult[]> result;

            if (string.Equals(idType, MetadataProviders.Zap2It.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                result = await _tvDbClientManager.GetSeriesByZap2ItIdAsync(id, cancellationToken);
            }
            else
            {
                result = await _tvDbClientManager.GetSeriesByImdbIdAsync(id, cancellationToken);
            }

            return result.Data.First().Id.ToString();
        }

        internal static bool IsValidSeries(Dictionary<string, string> seriesProviderIds)
        {
            return seriesProviderIds.TryGetValue(MetadataProviders.Tvdb.ToString(), out _) ||
                   seriesProviderIds.TryGetValue(MetadataProviders.Imdb.ToString(), out _) ||
                   seriesProviderIds.TryGetValue(MetadataProviders.Zap2It.ToString(), out _);
        }

        /// <summary>
        /// Finds the series.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="year">The year.</param>
        /// <param name="language">The language.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task{System.String}.</returns>
        private async Task<IEnumerable<RemoteSearchResult>> FindSeries(string name, int? year, string language, CancellationToken cancellationToken)
        {
            var results = await FindSeriesInternal(name, language, cancellationToken).ConfigureAwait(false);

            if (results.Count == 0)
            {
                var parsedName = _libraryManager.ParseName(name);
                var nameWithoutYear = parsedName.Name;

                if (!string.IsNullOrWhiteSpace(nameWithoutYear) && !string.Equals(nameWithoutYear, name, StringComparison.OrdinalIgnoreCase))
                {
                    results = await FindSeriesInternal(nameWithoutYear, language, cancellationToken).ConfigureAwait(false);
                }
            }

            return results.Where(i =>
            {
                if (year.HasValue && i.ProductionYear.HasValue)
                {
                    // Allow one year tolerance
                    return Math.Abs(year.Value - i.ProductionYear.Value) <= 1;
                }

                return true;
            });
        }

        private async Task<List<RemoteSearchResult>> FindSeriesInternal(string name, string language, CancellationToken cancellationToken)
        {
            _tvDbClientManager.TvDbClient.AcceptedLanguage = NormalizeLanguage(language);
            var comparableName = GetComparableName(name);
            var list = new List<Tuple<List<string>, RemoteSearchResult>>();
            TvDbResponse<SeriesSearchResult[]> result;
            try
            {
                result = await _tvDbClientManager.GetSeriesByNameAsync(comparableName, cancellationToken);
            }
            catch (TvDbServerException e)
            {
                _logger.LogError(e, "No series results found for {Name}", comparableName);
                return new List<RemoteSearchResult>();
            }

            foreach (var seriesSearchResult in result.Data)
            {
                var tvdbTitles = new List<string>
                {
                    GetComparableName(seriesSearchResult.SeriesName)
                };
                tvdbTitles.AddRange(seriesSearchResult.Aliases.Select(GetComparableName));

                DateTime.TryParse(seriesSearchResult.FirstAired, out var firstAired);
                var remoteSearchResult = new RemoteSearchResult
                {
                    Name = tvdbTitles.FirstOrDefault(),
                    ProductionYear = firstAired.Year,
                    SearchProviderName = Name,
                    ImageUrl = TVUtils.BannerUrl + seriesSearchResult.Banner

                };
                try
                {
                    var seriesSesult =
                        await _tvDbClientManager.GetSeriesByIdAsync(seriesSearchResult.Id, cancellationToken);
                    remoteSearchResult.SetProviderId(MetadataProviders.Imdb, seriesSesult.Data.ImdbId);
                    remoteSearchResult.SetProviderId(MetadataProviders.Zap2It, seriesSesult.Data.Zap2itId);
                }
                catch (TvDbServerException e)
                {
                    _logger.LogError(e, "Unable to retrieve series with id {TvdbId}", seriesSearchResult.Id);
                }

                remoteSearchResult.SetProviderId(MetadataProviders.Tvdb, seriesSearchResult.Id.ToString());
                list.Add(new Tuple<List<string>, RemoteSearchResult>(tvdbTitles, remoteSearchResult));
            }

            return list
                .OrderBy(i => i.Item1.Contains(comparableName, StringComparer.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(i => list.IndexOf(i))
                .Select(i => i.Item2)
                .ToList();
        }

        /// <summary>
        /// The remove
        /// </summary>
        const string remove = "\"'!`?";
        /// <summary>
        /// The spacers
        /// </summary>
        const string spacers = "/,.:;\\(){}[]+-_=–*";  // (there are not actually two - in the they are different char codes)

        /// <summary>
        /// Gets the name of the comparable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        private string GetComparableName(string name)
        {
            name = name.ToLowerInvariant();
            name = _localizationManager.NormalizeFormKD(name);
            var sb = new StringBuilder();
            foreach (var c in name)
            {
                if ((int)c >= 0x2B0 && (int)c <= 0x0333)
                {
                    // skip char modifier and diacritics
                }
                else if (remove.IndexOf(c) > -1)
                {
                    // skip chars we are removing
                }
                else if (spacers.IndexOf(c) > -1)
                {
                    sb.Append(" ");
                }
                else if (c == '&')
                {
                    sb.Append(" and ");
                }
                else
                {
                    sb.Append(c);
                }
            }
            name = sb.ToString();
            name = name.Replace(", the", "");
            name = name.Replace("the ", " ");
            name = name.Replace(" the ", " ");

            string prevName;
            do
            {
                prevName = name;
                name = name.Replace("  ", " ");
            } while (name.Length != prevName.Length);

            return name.Trim();
        }

        private static void MapSeriesToResult(MetadataResult<Series> result, TvDbSharper.Dto.Series tvdbSeries)
        {
            Series series = result.Item;
            series.SetProviderId(MetadataProviders.Tvdb, tvdbSeries.Id.ToString());
            series.Name = tvdbSeries.SeriesName;
            series.Overview = (tvdbSeries.Overview ?? string.Empty).Trim();
            result.ResultLanguage = TvDbClientManager.Instance.TvDbClient.AcceptedLanguage;
            series.AirDays = TVUtils.GetAirDays(tvdbSeries.AirsDayOfWeek);
            series.AirTime = tvdbSeries.AirsTime;

            series.CommunityRating = (float?)tvdbSeries.SiteRating;
            series.SetProviderId(MetadataProviders.Imdb, tvdbSeries.ImdbId);
            series.SetProviderId(MetadataProviders.Zap2It, tvdbSeries.Zap2itId);
            if (Enum.TryParse(tvdbSeries.Status, true, out SeriesStatus seriesStatus))
            {
                series.Status = seriesStatus;
            }

            if (DateTime.TryParse(tvdbSeries.FirstAired, out var date))
            {
                date = date.ToUniversalTime();

                series.PremiereDate = date;
                series.ProductionYear = date.Year;
            }

            series.RunTimeTicks = TimeSpan.FromMinutes(Convert.ToDouble(tvdbSeries.Runtime)).Ticks;
            foreach (var genre in tvdbSeries.Genre)
            {
                series.AddGenre(genre);
            }

            series.AddStudio(tvdbSeries.Network);

            // TODO is this necessary?
            // if (result.Item.Status.HasValue && result.Item.Status.Value == SeriesStatus.Ended && episodeAirDates.Count > 0)
            // {
            //     result.Item.EndDate = episodeAirDates.Max();
            // }
        }

        private static void MapActorsToResult(MetadataResult<Series> result, IEnumerable<Actor> actors)
        {
            foreach (Actor actor in actors)
            {
                var personInfo = new PersonInfo
                {
                    Type = PersonType.Actor,
                    Name = (actor.Name ?? string.Empty).Trim(),
                    Role = actor.Role,
                    ImageUrl = actor.Image,
                    SortOrder = actor.SortOrder
                };

                if (!string.IsNullOrWhiteSpace(personInfo.Name))
                {
                    result.AddPerson(personInfo);
                }
            }
        }

        public string Name => "TheTVDB";

        public async Task Identify(SeriesInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.GetProviderId(MetadataProviders.Tvdb)))
            {
                return;
            }

            var srch = await FindSeries(info.Name, info.Year, info.MetadataLanguage, CancellationToken.None).ConfigureAwait(false);

            var entry = srch.FirstOrDefault();

            if (entry != null)
            {
                var id = entry.GetProviderId(MetadataProviders.Tvdb);
                info.SetProviderId(MetadataProviders.Tvdb, id);
            }
        }

        public int Order => 0;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url,
                BufferContent = false
            });
        }
    }
}
