using System;
using System.Threading.Tasks;

namespace Gribble.Extensions
{
    public static class ObjectExtensions
    {
        public static T FromDb<T>(this object value)
        {
            if (value is int && typeof(T) == typeof(bool))
                return (T)(object)((int) value != 0);
            return value == DBNull.Value ? default(T) : (T)value;
        }

        public static async Task<T> FromDbAsync<T>(this Task<object> result)
        {
            var value = await result;
            if (value is int && typeof(T) == typeof(bool))
                return (T)(object)((int) value != 0);
            return value == DBNull.Value ? default(T) : (T)value;
        }
    }
}
