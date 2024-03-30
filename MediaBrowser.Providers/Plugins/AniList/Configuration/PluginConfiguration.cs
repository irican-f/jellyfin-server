using MediaBrowser.Model.Plugins;

namespace MediaBrowser.Providers.Plugins.AniList.Configuration
{
    /// <summary>
    /// Preferences for anime title.
    /// </summary>
    public enum TitlePreferenceType
    {
        /// <summary>
        /// Use titles in the local metadata language.
        /// </summary>
        Localized,

        /// <summary>
        /// Use titles in Japanese.
        /// </summary>
        Japanese,

        /// <summary>
        /// Use titles in Japanese romaji.
        /// </summary>
        JapaneseRomaji
    }

    /// <summary>
    /// Default anime genre.
    /// </summary>
    public enum AnimeDefaultGenreType
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Anime.
        /// </summary>
        Anime,

        /// <summary>
        /// Animation.
        /// </summary>
        Animation
    }

    /// <summary>
    /// Plugin configuration class.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            TitlePreference = TitlePreferenceType.Localized;
            OriginalTitlePreference = TitlePreferenceType.JapaneseRomaji;
            MaxGenres = 5;
            AnimeDefaultGenre = AnimeDefaultGenreType.Anime;
            AniDbRateLimit = 2000;
            AniDbReplaceGraves = true;
            AniListShowSpoilerTags = true;
            UseAnitomyLibrary = false;
        }

        /// <summary>
        /// Gets or Sets the Title preference.
        /// </summary>
        public TitlePreferenceType TitlePreference { get; set; }

        /// <summary>
        /// Gets or Sets the OriginalTitle preference.
        /// </summary>
        public TitlePreferenceType OriginalTitlePreference { get; set; }

        /// <summary>
        /// Gets or Sets the max number of genres to fetch.
        /// </summary>
        public int MaxGenres { get; set; }

        /// <summary>
        /// Gets or Sets the default anime genre.
        /// </summary>
        public AnimeDefaultGenreType AnimeDefaultGenre { get; set; }

        /// <summary>
        /// Gets or Sets the AniDb API rate limit threshold.
        /// </summary>
        public int AniDbRateLimit { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether AniDb should replace graves.
        /// </summary>
        public bool AniDbReplaceGraves { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether AniList should show spoiler tags.
        /// </summary>
        public bool AniListShowSpoilerTags { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the plugin should filter people by title preference.
        /// </summary>
        public bool FilterPeopleByTitlePreference { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating whether the plugin should use the Anitomy libraru for title resolve.
        /// </summary>
        public bool UseAnitomyLibrary { get; set; }
    }
}
