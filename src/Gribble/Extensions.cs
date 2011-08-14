using System;
using System.Collections.Generic;
using System.Linq;
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
            items.ToList().ForEach(x => source.Add(x.Key, x.Value));
        }
    }
}
