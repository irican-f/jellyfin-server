using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Providers.Plugins.AniList.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1402
#pragma warning disable CA2227
#pragma warning disable CA1002
#pragma warning disable CA1721
#pragma warning disable CA1305

namespace MediaBrowser.Providers.Plugins.AniList.Providers.AniList
{
    using System.Collections.Generic;

    /// <summary>
    /// Represent a title in different types/languages.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Not my code")]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Title
    {
        /// <summary>
        /// Gets or sets japanese title in latin letters.
        /// </summary>
        public string? Romaji { get; set; }

        /// <summary>
        /// Gets or sets english title.
        /// </summary>
        public string? English { get; set; }

        /// <summary>
        /// Gets or sets native title.
        /// </summary>
        public string? Native { get; set; }
    }

    /// <summary>
    /// Cover image.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CoverImage
    {
        /// <summary>
        /// Gets or sets the url to the medium image.
        /// </summary>
        public string? Medium { get; set; }

        /// <summary>
        /// Gets or sets the url to the large image.
        /// </summary>
        public string? Large { get; set; }

        /// <summary>
        /// Gets or sets the url to the extra large image.
        /// </summary>
        public string? ExtraLarge { get; set; }
    }

    /// <summary>
    /// Represents ApiDate.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ApiDate
    {
        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Gets or sets the month.
        /// </summary>
        public int? Month { get; set; }

        /// <summary>
        /// Gets or sets the day.
        /// </summary>
        public int? Day { get; set; }
    }

    /// <summary>
    /// Represent an API search result page.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Page
    {
        /// <summary>
        /// Gets or sets the search results.
        /// </summary>
        public List<MediaSearchResult>? Media { get; set; }
    }

    public class Data
    {
        public Page Page { get; set; }

        [JsonProperty("media")]
        public Media? Media { get; set; }
    }

    /// <summary>
    /// A slimmed down version of Media to avoid confusion and reduce
    /// the size of responses when searching.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class MediaSearchResult
    {
        public int Id { get; set; }

        public Title Title { get; set; }

        public ApiDate StartDate { get; set; }

        public CoverImage CoverImage { get; set; }

        /// <summary>
        /// Get the title in configured language.
        /// </summary>
        /// <param name="preference">Title preference.</param>
        /// <param name="language">Language to get title in.</param>
        /// <returns>The preferred title or null.</returns>
        public string? GetPreferredTitle(TitlePreferenceType preference, string language)
        {
            return preference switch
            {
                TitlePreferenceType.Localized when language == "en" => this.Title.English,
                TitlePreferenceType.Localized when language == "jap" => this.Title.Native,
                TitlePreferenceType.Japanese => this.Title.Native,
                _ => this.Title.Romaji
            };
        }

        /// <summary>
        /// Get the highest quality image url.
        /// </summary>
        /// <returns>The url to the best quality image.</returns>
        public string? GetImageUrl()
        {
            return this.CoverImage.ExtraLarge ?? this.CoverImage.Large ?? this.CoverImage.Medium;
        }

        /// <summary>
        /// Returns the start date as a DateTime object or null if not available.
        /// </summary>
        /// <returns>The start date of the anime.</returns>
        public DateTime? GetStartDate()
        {
            if (this.StartDate.Year == null || this.StartDate.Month == null || this.StartDate.Day == null)
            {
                return null;
            }

            return new DateTime(this.StartDate.Year.Value, this.StartDate.Month.Value, this.StartDate.Day.Value);
        }

        /// <summary>
        /// Convert a Media/MediaSearchResult object to a RemoteSearchResult.
        /// </summary>
        /// <returns>The RemoteSearchResult.</returns>
        public RemoteSearchResult ToSearchResult()
        {
            PluginConfiguration config = Plugin.Instance?.Configuration!;
            return new RemoteSearchResult
            {
                Name = this.GetPreferredTitle(config.TitlePreference, "en"),
                ProductionYear = this.StartDate.Year,
                PremiereDate = this.GetStartDate(),
                ImageUrl = this.GetImageUrl(),
                SearchProviderName = ProviderNames.AniList,
                ProviderIds = new Dictionary<string, string>()
                {
                    { ProviderNames.AniList, this.Id.ToString() }
                }
            };
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Media : MediaSearchResult
    {
        public int? AverageScore { get; set; }

        public string BannerImage { get; set; }

        public object Chapters { get; set; }

        public Characters Characters { get; set; }

        public string Description { get; set; }

        public int? Duration { get; set; }

        public ApiDate EndDate { get; set; }

        public int? Episodes { get; set; }

        public string Format { get; set; }

        public List<string> Genres { get; set; }

        public object Hashtag { get; set; }

        public bool IsAdult { get; set; }

        public int? MeanScore { get; set; }

        public object NextAiringEpisode { get; set; }

        public int? Popularity { get; set; }

        public string Season { get; set; }

        public int? SeasonYear { get; set; }

        public string Status { get; set; }

        public StudioConnection Studios { get; set; }

        public List<object> Synonyms { get; set; }

        public List<Tag> Tags { get; set; }

        public string Type { get; set; }

        public object Volumes { get; set; }

        /// <summary>
        /// Get the rating, normalized to 1-10.
        /// </summary>
        /// <returns>The rating.</returns>
        public float GetRating()
        {
            return (this.AverageScore ?? 0) / 10f;
        }

        /// <summary>
        /// Returns the end date as a DateTime object or null if not available.
        /// </summary>
        /// <returns>End date if anime is finished.</returns>
        public DateTime? GetEndDate()
        {
            if (this.EndDate.Year == null || this.EndDate.Month == null || this.EndDate.Day == null)
            {
                return null;
            }

            return new DateTime(this.EndDate.Year.Value, this.EndDate.Month.Value, this.EndDate.Day.Value);
        }

        /// <summary>
        /// Returns a list of studio names.
        /// </summary>
        /// <returns>Studio names list.</returns>
        public List<string> GetStudioNames()
        {
            return this.Studios.Nodes.Select(node => node.Name).ToList();
        }

        /// <summary>
        /// Returns a list of PersonInfo for voice actors.
        /// </summary>
        /// <returns>Voice actors info list.</returns>
        public List<PersonInfo> GetPeopleInfo()
        {
            PluginConfiguration config = Plugin.Instance?.Configuration!;
            List<PersonInfo> personInfoList = new List<PersonInfo>();
            foreach (CharacterEdge edge in this.Characters.Edges)
            {
                foreach (VoiceActor va in edge.VoiceActors)
                {
                    if (config.FilterPeopleByTitlePreference)
                    {
                        if (config.TitlePreference != TitlePreferenceType.Localized && va.Language != "JAPANESE")
                        {
                            continue;
                        }

                        if (config.TitlePreference == TitlePreferenceType.Localized && va.Language == "JAPANESE")
                        {
                            continue;
                        }
                    }

                    var foundPersonType = Enum.TryParse<PersonKind>(PersonType.Actor, out var personKind);

                    if (!foundPersonType)
                    {
                        personKind = PersonKind.Actor;
                    }

                    PeopleHelper.AddPerson(personInfoList, new PersonInfo
                    {
                        Name = va.Name.Full,
                        ImageUrl = va.Image.Large ?? va.Image.Medium,
                        Role = edge.Node.Name.Full,
                        Type = personKind,
                        ProviderIds = new Dictionary<string, string>()
                        {
                            { ProviderNames.AniList, this.Id.ToString() }
                        }
                    });
                }
            }

            return personInfoList;
        }

        /// <summary>
        /// Returns a list of tag names.
        /// </summary>
        /// <returns>Tag names.</returns>
        public List<string> GetTagNames()
        {
            PluginConfiguration config = Plugin.Instance?.Configuration!;
            return (from tag in this.Tags where config.AniListShowSpoilerTags || !tag.IsMediaSpoiler select tag.Name).ToList();
        }

        /// <summary>
        /// Returns a list of genres.
        /// </summary>
        /// <returns>Genre list.</returns>
        public List<string> GetGenres()
        {
            PluginConfiguration config = Plugin.Instance?.Configuration!;

            if (config.AnimeDefaultGenre != AnimeDefaultGenreType.None)
            {
                this.Genres = this.Genres
                    .Except(new[] { "Animation", "Anime" })
                    .Prepend(config.AnimeDefaultGenre.ToString())
                    .ToList();
            }

            if (config.MaxGenres > 0)
            {
                this.Genres = this.Genres.Take(config.MaxGenres).ToList();
            }

            return this.Genres.OrderBy(i => i).ToList();
        }

        /// <summary>
        /// Convert a Media object to a Series.
        /// </summary>
        /// <returns>Series object.</returns>
        public Series ToSeries()
        {
            PluginConfiguration config = Plugin.Instance?.Configuration!;
            var result = new Series
            {
                Name = this.GetPreferredTitle(config.TitlePreference, "en"),
                OriginalTitle = this.GetPreferredTitle(config.OriginalTitlePreference, "en"),
                Overview = this.Description,
                ProductionYear = this.StartDate.Year,
                PremiereDate = this.GetStartDate(),
                EndDate = this.GetStartDate(),
                CommunityRating = this.GetRating(),
                RunTimeTicks = this.Duration.HasValue ? TimeSpan.FromMinutes(this.Duration.Value).Ticks : (long?)null,
                Genres = this.GetGenres().ToArray(),
                Tags = this.GetTagNames().ToArray(),
                Studios = this.GetStudioNames().ToArray(),
                ProviderIds = new Dictionary<string, string>()
                {
                    { ProviderNames.AniList, this.Id.ToString() }
                }
            };

            if (this.Status == "FINISHED" || this.Status == "CANCELLED")
            {
                result.Status = SeriesStatus.Ended;
            }
            else if (this.Status == "RELEASING")
            {
                result.Status = SeriesStatus.Continuing;
            }

            return result;
        }

        /// <summary>
        /// Convert a Media object to a Movie.
        /// </summary>
        /// <returns>Movie object.</returns>
        public Movie ToMovie()
        {
            PluginConfiguration config = Plugin.Instance?.Configuration!;
            return new Movie
            {
                Name = this.GetPreferredTitle(config.TitlePreference, "en"),
                OriginalTitle = this.GetPreferredTitle(config.OriginalTitlePreference, "en"),
                Overview = this.Description,
                ProductionYear = this.StartDate.Year,
                PremiereDate = this.GetStartDate(),
                EndDate = this.GetStartDate(),
                CommunityRating = this.GetRating(),
                Genres = this.Genres.ToArray(),
                Tags = this.GetTagNames().ToArray(),
                Studios = this.GetStudioNames().ToArray(),
                ProviderIds = new Dictionary<string, string>()
                {
                    { ProviderNames.AniList, this.Id.ToString() }
                }
            };
        }
    }

    public class PageInfo
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("perPage")]
        public int PerPage { get; set; }

        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("lastPage")]
        public int LastPage { get; set; }
    }

    public class Name
    {
        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }
    }

    public class Image
    {
        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }
    }

    public class Character
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public Name2 Name { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }
    }

    public class Name2
    {
        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }

        [JsonProperty("full")]
        public string Full { get; set; }

        [JsonProperty("native")]
        public string Native { get; set; }
    }

    public class VoiceActor
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public Name2 Name { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }

    public class CharacterEdge
    {
        [JsonProperty("node")]
        public Character Node { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("voiceActors")]
        public List<VoiceActor> VoiceActors { get; set; }
    }

    public class Characters
    {
        [JsonProperty("edges")]
        public List<CharacterEdge> Edges { get; set; }
    }

    public class Tag
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("isMediaSpoiler")]
        public bool IsMediaSpoiler { get; set; }
    }

    public class Studio
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isAnimationStudio")]
        public bool IsAnimationStudio { get; set; }
    }

    public class StudioConnection
    {
        [JsonProperty("nodes")]
        public List<Studio> Nodes { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}

#pragma warning restore SA1402
#pragma warning restore CA2227
#pragma warning restore CA1002
#pragma warning restore CA1721
#pragma warning restore CA1305
