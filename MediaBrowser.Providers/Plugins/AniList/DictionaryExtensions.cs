using System.Collections.Generic;

namespace MediaBrowser.Providers.Plugins.AniList
{
    /// <summary>
    /// Extension methods for Dictionary class.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value or return the default value of the type.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">Key to lookup.</param>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <returns>The value or its default value.</returns>
        public static T? GetOrDefault<TKey, T>(this IDictionary<TKey, T?> dict, TKey key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }

            return default(T);
        }
    }
}
