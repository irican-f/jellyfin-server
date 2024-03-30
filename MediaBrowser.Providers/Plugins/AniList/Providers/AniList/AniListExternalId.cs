using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace MediaBrowser.Providers.Plugins.AniList.Providers.AniList
{
    /// <inheritdoc />
    public class AniListExternalId : IExternalId
    {
        /// <inheritdoc />
        public string ProviderName
            => ProviderNames.AniList;

        /// <inheritdoc />
        public string Key
            => ProviderNames.AniList;

        /// <inheritdoc />
        public ExternalIdMediaType? Type
            => ExternalIdMediaType.Series;

        /// <inheritdoc />
        public string UrlFormatString
            => "https://anilist.co/anime/{0}/";

        /// <inheritdoc />
        public bool Supports(IHasProviderIds item)
            => item is Series || item is Movie;
    }
}
