using System;
using System.Collections.Generic;

namespace Gribble.TransactSql
{
    public static class DictionaryExtensions
    {
        private static readonly Random Random = new Random();

        public static string AddWithRandomlyNamedKey<TValue>(this IDictionary<string, TValue> dictionary, TValue value)
        {
            var name = "K" + Random.Next();
            dictionary.Add(name, value);
            return name;
        }
    }
}
