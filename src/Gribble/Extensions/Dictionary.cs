using System;
using System.Collections.Generic;
using System.Linq;

namespace Gribble.Extensions
{
    public static class DictionaryExtensions
    {
        public static string AddWithUniquelyNamedKey<TValue>(
            this IDictionary<string, TValue> dictionary, TValue value)
        {
            var name = "K" + Unique.Next();
            dictionary.Add(name, value);
            return name;
        }

        public static void AddRange<TKey, TValue>(
            this IDictionary<TKey, TValue> source, 
            IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            items.ToList().ForEach(x => source.Add(x.Key, x.Value));
        }

        public static TValue AddItem<TKey, TValue>(
            this IDictionary<TKey, TValue> source, 
            TKey key, TValue value)
        {
            source.Add(key, value);
            return value;
        }

        public static bool ContainsKey<TKey, TValue>(this IDictionary<TKey, TValue> source, 
            TKey key, Func<TKey, TKey> mapKey)
        {
            return source.Keys.Any(x => mapKey(x)?.Equals(key) ?? false);
        }

        public static TValue Map<TKey, TValue>(this IDictionary<TKey, TValue> source, 
            TKey key, Func<TKey, TKey> mapKey)
        {
            var realKey = source.Keys.FirstOrDefault(x => mapKey(x)?.Equals(key) ?? false);
            if (realKey == null)
                throw new NullReferenceException($"Key '{key}' mapping not found.");
            return source[realKey];
        }

        public static Dictionary<TKey, TValue> ToDistinctDictionary<T, TKey, TValue>(
            this IEnumerable<T> source, Func<T, TKey> keySelector, 
            Func<T, TValue> valueSelector, 
            IEqualityComparer<TKey> equalityComparer = null)
        {
            var dictionary = equalityComparer != null
                ? new Dictionary<TKey, TValue>(equalityComparer)
                : new Dictionary<TKey, TValue>();
            source.ForEach(x => dictionary[keySelector(x)] = valueSelector(x));
            return dictionary;
        }
    }
}
