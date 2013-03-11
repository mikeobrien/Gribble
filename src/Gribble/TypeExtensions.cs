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

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        public static bool IsPrimitive(this Type type, bool includeNullable)
        {
            
            return !includeNullable ? type.IsPrimitive : (type.IsNullable() ? Nullable.GetUnderlyingType(type).IsPrimitive : type.IsPrimitive);
        }

        public static bool IsType<T>(this Type type, bool includeNullable)
        {
            return !includeNullable ? type == typeof(T) : (type.IsNullable() ? Nullable.GetUnderlyingType(type) == typeof(T) : type == typeof(T));
        }
    }
}
