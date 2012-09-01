using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gribble
{
    public static class ReflectionExtensions
    {
        private static readonly Func<Type, IEnumerable<PropertyInfo>> PrimitiveProperties = 
            Func.Memoize<Type, IEnumerable<PropertyInfo>>(t => t.GetProperties()
                .Where(x => x.PropertyType.IsPrimitive || 
                            x.PropertyType == typeof(string) ||
                            x.PropertyType == typeof(Guid) || 
                            x.PropertyType == typeof(DateTime) || 
                            x.PropertyType == typeof(TimeSpan))); 

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source == null ? new Dictionary<string, object>() : 
                PrimitiveProperties(source.GetType()).ToDictionary(x => x.Name, x => x.GetValue(source, null));
        }
    }
}