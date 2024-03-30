using System.Text.RegularExpressions;

namespace MediaBrowser.Providers.Plugins.AniList.Providers.AniList
{
    /// <summary>
    /// Search Helper for AniList.
    /// </summary>
    public static class AnilistSearchHelper
    {
        /// <summary>
        /// Preprocess the title by the path of the file.
        /// </summary>
        /// <param name="path">Path to process.</param>
        /// <returns>The preprocessed title.</returns>
        public static string PreprocessTitle(string path)
        {
            // Remove items that will always cause anilist to fail
            var input = path;

            // Season designation
            input = Regex.Replace(input, @"(\s|\.)S[0-9]{1,2}", string.Empty);
            // ~ ALT NAME ~
            input = Regex.Replace(input, @"\s*~(\w|[0-9]|\s)+~", string.Empty);

            // Native Name (English Name)
            // Only replaces if the name ends with a parenthesis to hopefully avoid mangling titles with parens (e.g. Evangelion 1.11 You Are (Not) Alone)
            input = Regex.Replace(input.Trim(), @"\((\w|[0-9]|\s)+\)$", string.Empty);

            return input;
        }
    }
}
