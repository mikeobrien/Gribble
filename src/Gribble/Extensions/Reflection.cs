using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gribble.Extensions
{
    public static class ReflectionExtensions
    {
        private static readonly Func<Type, IEnumerable<PropertyInfo>> SimpleTypeProperties = 
            Func.Memoize<Type, IEnumerable<PropertyInfo>>(t => t.GetProperties()
                .Where(x => x.PropertyType.IsSimpleType()));

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source == null ? new Dictionary<string, object>() :
                SimpleTypeProperties(source.GetType()).ToDictionary(x =>
                    x.Name, x => x.GetValue(source, null));
        }

        public static bool IsSimpleType(this Type type)
        {
            bool SimpleType(Type x) => x.IsPrimitive(true) || x == typeof(string) || 
                x == typeof(decimal) || x == typeof(DateTime) || x == typeof(Guid) || 
                x == typeof(TimeSpan);

            return SimpleType(type) || (type.IsNullable() && 
                SimpleType(Nullable.GetUnderlyingType(type)));
        }

        public static bool IsSameTypeAs(this Type source, Type compare)
        {
            return source.UnderlyingSystemType == compare.UnderlyingSystemType;
        }

        public static Type GetUnderlyingType(this Type type)
        {
            var underlyingType = type.GetNullableType() ?? type;
            return underlyingType.IsEnum ? underlyingType.GetEnumUnderlyingType() : underlyingType;
        }

        public static Type GetNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }
    }
}