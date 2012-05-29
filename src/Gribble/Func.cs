using System;
using System.Collections.Generic;

namespace Gribble
{
    public static class Func
    {
        public static Func<T1, T2> Memoize<T1, T2>(Func<T1, T2> func)
        {
            var map = new Dictionary<T1, T2>();
            return x => {
                if (map.ContainsKey(x)) return map[x];
                var result = func(x);
                map.Add(x, result);
                return result;
            };
        }
    }
}