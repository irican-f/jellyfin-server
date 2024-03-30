using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace MediaBrowser.Providers.Plugins.AniList.Providers.AniList
{
    /// <summary>
    /// AniList image provider.
    /// </summary>
    public class AniListImageProvider : IRemoteImageProvider
    {
        private readonly AniListApi _aniListApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="AniListImageProvider"/> class.
        /// </summary>
        public AniListImageProvider()
        {
            _aniListApi = new AniListApi();
        }

        /// <inheritdoc />
        public string Name => "AniList";

        /// <inheritdoc />
        public bool Supports(BaseItem item) => item is Series || item is Season || item is Movie;

        /// <inheritdoc />
        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new[] { ImageType.Primary, ImageType.Backdrop };
        }

        /// <inheritdoc />
        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var seriesId = item.GetProviderId(ProviderNames.AniList);

            if (seriesId == null)
            {
                return Task.FromResult<IEnumerable<RemoteImageInfo>>(new List<RemoteImageInfo>());
            }

            return GetImages(seriesId, cancellationToken);
        }

        /// <summary>
        /// Get image for anime by AniList anime id.
        /// </summary>
        /// <param name="aid">Anime id on AniList.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>List of images for the anime.</returns>
        public async Task<IEnumerable<RemoteImageInfo>> GetImages(string aid, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            if (!string.IsNullOrEmpty(aid))
            {
                Media? media = await _aniListApi.GetAnime(aid).ConfigureAwait(false);
                if (media != null)
                {
                    if (media.GetImageUrl() != null)
                    {
                        list.Add(new RemoteImageInfo
                        {
                            ProviderName = Name,
                            Type = ImageType.Primary,
                            Url = media.GetImageUrl()
                        });
                    }

                    if (media.BannerImage != null)
                    {
                        list.Add(new RemoteImageInfo
                        {
                            ProviderName = Name,
                            Type = ImageType.Backdrop,
                            Url = media.BannerImage
                        });
                    }
                }
            }

            return list;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            var httpClient = Plugin.Instance?.GetHttpClient()!;

            return await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }
}
