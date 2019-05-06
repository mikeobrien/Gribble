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
    }
}
