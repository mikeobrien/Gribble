using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gribble.Extensions
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

        public static bool IsType<T>(this Type type, bool includeNullable = true)
        {
            return !includeNullable ? type == typeof(T) : (type.IsNullable() ? Nullable.GetUnderlyingType(type) == typeof(T) : type == typeof(T));
        }

        public static bool IsString(this Type type)
        {
            return type.IsType<string>();
        }

        public static bool IsGuid(this Type type, bool nullable = true)
        {
            return type.IsType<Guid>(nullable);
        }

        public static bool IsDateTime(this Type type, bool nullable = true)
        {
            return type.IsType<DateTime>(nullable);
        }

        public static bool IsTimeSpan(this Type type, bool nullable = true)
        {
            return type.IsType<TimeSpan>(nullable);
        }

        public static bool IsInteger(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
