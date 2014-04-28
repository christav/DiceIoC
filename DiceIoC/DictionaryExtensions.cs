using System.Collections.Generic;

namespace DiceIoC
{
    static class DictionaryExtensions
    {
        /// <summary>
        /// Try to get a value from a dictionary, return default(TValue) if not present.
        /// </summary>
        /// <typeparam name="TKey">Type of dictionary key</typeparam>
        /// <typeparam name="TValue">Type of dictionary value.</typeparam>
        /// <param name="dict">Dictionary to query.</param>
        /// <param name="key">Key to look up.</param>
        /// <param name="defaultValue">Default value to return, defaults to default(TValue)</param>
        /// <returns>Value in the dictionary at key, or default(TValue) if not present.</returns>
        internal static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            TValue result;
            if (dict.TryGetValue(key, out result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}
