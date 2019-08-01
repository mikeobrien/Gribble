using System;
using System.Collections.Generic;
using System.Linq;

namespace Gribble.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> UnionOrDefault<T>(
            this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null) return second;
            if (second == null) return first;
            return first.Union(second);
        }

        public static IEnumerable<int> ToRange(this int count)
        {
            return Enumerable.Range(0, count);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}
