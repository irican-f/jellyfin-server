using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MediaBrowser.Providers.Plugins.AniList.Providers.AniList
{
    /// <summary>
    /// Based on the new API from AniList
    /// 🛈 This code works with the API Interface (v2) from AniList
    /// 🛈 https://anilist.gitbooks.io/anilist-apiv2-docs
    /// 🛈 THIS IS AN UNOFFICAL API INTERFACE FOR JELLYFIN.
    /// </summary>
    [SuppressMessage("Globalization", "CA1307:Spécifier StringComparison pour plus de clarté", Justification = "Not my code")]
    [SuppressMessage("Globalization", "CA1305:Spécifier IFormatProvider", Justification = "Not my code")]
    public class AniListApi
    {
        private const string SearchLink = @"https://graphql.anilist.co/api/v2?query=
query ($query: String, $type: MediaType) {
  Page {
    media(search: $query, type: $type) {
      id
      title {
        romaji
        english
        native
      }
      coverImage {
        medium
        large
        extraLarge
      }
      startDate {
        year
        month
        day
      }
    }
  }
}&variables={ ""query"":""{0}"",""type"":""ANIME""}";

        private const string AnimeLink = @"https://graphql.anilist.co/api/v2?query=
query($id: Int!, $type: MediaType) {
  Media(id: $id, type: $type) {
    id
    title {
      romaji
      english
      native
      userPreferred
    }
    startDate {
      year
      month
      day
    }
    endDate {
      year
      month
      day
    }
    coverImage {
      medium
      large
      extraLarge
    }
    bannerImage
    format
    type
    status
    episodes
    chapters
    volumes
    season
    seasonYear
    description
    averageScore
    meanScore
    genres
    synonyms
    duration
    tags {
      id
      name
      category
      isMediaSpoiler
    }
    nextAiringEpisode {
      airingAt
      timeUntilAiring
      episode
    }

    studios {
      nodes {
        id
        name
        isAnimationStudio
      }
    }
    characters(sort: [ROLE]) {
      edges {
        node {
          id
          name {
            first
            last
            full
          }
          image {
            medium
            large
          }
        }
        role
        voiceActors {
          id
          name {
            first
            last
            full
            native
          }
          image {
            medium
            large
          }
          language
        }
      }
    }
  }
}&variables={ ""id"":""{0}"",""type"":""ANIME""}";

        static AniListApi()
        {
        }

        /// <summary>
        /// API call to get the anime with the given id.
        /// </summary>
        /// <param name="id">Id of the anime.</param>
        /// <returns>Media instance.</returns>
        public async Task<Media?> GetAnime(string id)
        {
            return (await WebRequestApi(AnimeLink.Replace("{0}", id)).ConfigureAwait(false))?.Data.Media;
        }

        /// <summary>
        /// API call to search a title and return the first result.
        /// </summary>
        /// <param name="title">Title to search.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>Instance of MediaSearchResult.</returns>
        public async Task<MediaSearchResult?> SearchGetSeries(string title, CancellationToken cancellationToken)
        {
            // Reimplemented instead of calling Search_GetSeries_list() for efficiency
            var webContent = await WebRequestApi(SearchLink.Replace("{0}", title)).ConfigureAwait(false);

            return webContent?.Data.Page.Media?.FirstOrDefault();
        }

        /// <summary>
        /// API call to search a title and return a list of results.
        /// </summary>
        /// <param name="title">Title to search.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>List of MediaSearchResult instances.</returns>
        public async Task<List<MediaSearchResult>?> SearchGetSeriesList(string title, CancellationToken cancellationToken)
        {
            return (await WebRequestApi(SearchLink.Replace("{0}", title)).ConfigureAwait(false))?.Data.Page.Media;
        }

        /// <summary>
        /// Search for anime with the given title. Attempts to fuzzy search by removing special characters.
        /// </summary>
        /// <param name="title">Title to search.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>The anime id on AniList.</returns>
        public async Task<string?> FindSeries(string title, CancellationToken cancellationToken)
        {
            var result = await SearchGetSeries(title, cancellationToken).ConfigureAwait(false);

            if (result != null)
            {
                return result.Id.ToString();
            }

            result = await SearchGetSeries(await EqualsCheck.ClearName(title, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
            if (result != null)
            {
                return result.Id.ToString();
            }

            return null;
        }

        /// <summary>
        /// GET and parse JSON content from link, deserialize into a RootObject.
        /// </summary>
        /// <param name="link">The link to make a request to.</param>
        /// <returns>RootObject instance.</returns>
        public async Task<RootObject?> WebRequestApi(string link)
        {
            using var httpClient = Plugin.Instance?.GetHttpClient();

            if (httpClient == null)
            {
                return null;
            }

            using HttpContent content = new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>());
            using var response = await httpClient.PostAsync(link, content).ConfigureAwait(false);
            var responseJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<RootObject>(responseJsonString);
        }
    }
}
