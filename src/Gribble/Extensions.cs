using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gribble
{
    public static class Extensions
    {
        public static string NormalizeWhitespace(this string value)
        {
            return Regex.Replace(value, @"\s+", " ");
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            items.Run(x => source.Add(x.Key, x.Value));
        }

        // Stole this from the Rx extensions. Decided not to create a dependency 
        // on Rx since this was the only method used in this library.
        public static void Run<T>(this IEnumerable<T> items, Action<T> onNext)
        {
            foreach (var item in items) onNext(item);
        }
    }
}
