using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gribble
{
    public static class TypeExtensions
    {
        private static readonly Func<Type, IEnumerable<PropertyInfo>> GetProperties =
            Func.Memoize<Type, IEnumerable<PropertyInfo>>(x => x.GetProperties());

        public static IEnumerable<PropertyInfo> GetCachedProperties(this Type type)
        {
            return GetProperties(type);
        }
    }
}
