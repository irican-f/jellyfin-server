using System.Linq;

using AnitomySharp;

namespace MediaBrowser.Providers.Plugins.AniList.Providers
{
    /// <summary>
    /// AnitomySharp helper class.
    /// </summary>
    public static class AnitomyHelper
    {
        /// <summary>
        /// Extract the anime title from the path.
        /// </summary>
        /// <param name="path">Path to extract the title from.</param>
        /// <returns>Anime title or null.</returns>
        public static string? ExtractAnimeTitle(string path)
        {
            var input = path;
            var elements = AnitomySharp.AnitomySharp.Parse(input);

            return elements.FirstOrDefault(p => p.Category == Element.ElementCategory.ElementAnimeTitle)?.Value;
        }

        /// <summary>
        /// Extract the episode title from the path.
        /// </summary>
        /// <param name="path">Path to extract the episode title from.</param>
        /// <returns>Episode title or null.</returns>
        public static string? ExtractEpisodeTitle(string path)
        {
            var elements = AnitomySharp.AnitomySharp.Parse(path);
            return elements.FirstOrDefault(p => p.Category == Element.ElementCategory.ElementEpisodeTitle)?.Value;
        }

        /// <summary>
        /// Extract the episode number from the path.
        /// </summary>
        /// <param name="path">Path to extract the episode number from.</param>
        /// <returns>Episode number or null.</returns>
        public static string? ExtractEpisodeNumber(string path)
        {
            var elements = AnitomySharp.AnitomySharp.Parse(path);
            return elements.FirstOrDefault(p => p.Category == Element.ElementCategory.ElementEpisodeNumber)?.Value;
        }

        /// <summary>
        /// Extract the season number from the path.
        /// </summary>
        /// <param name="path">Path to extract the season number from.</param>
        /// <returns>Season number or null.</returns>
        public static string? ExtractSeasonNumber(string path)
        {
            var elements = AnitomySharp.AnitomySharp.Parse(path);
            return elements.FirstOrDefault(p => p.Category == Element.ElementCategory.ElementAnimeSeason)?.Value;
        }
    }
}
