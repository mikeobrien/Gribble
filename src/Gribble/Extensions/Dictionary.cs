using System;
using System.Collections.Generic;
using System.Linq;

namespace Gribble.Extensions
{
    public static class DictionaryExtensions
    {
        private static readonly Random Random = new Random();

        public static string AddWithRandomlyNamedKey<TValue>(
            this IDictionary<string, TValue> dictionary, TValue value)
        {
            var name = "K" + Random.Next();
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
    }
}
