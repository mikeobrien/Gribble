using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gribble.Extensions
{
    public static class ReflectionExtensions
    {
        private static readonly Func<Type, IEnumerable<PropertyInfo>> PrimitiveProperties = 
            Func.Memoize<Type, IEnumerable<PropertyInfo>>(t => t.GetProperties()
                .Where(x => x.PropertyType.IsPrimitive(true) ||
                            x.PropertyType.IsString() ||
                            x.PropertyType.IsGuid() || 
                            x.PropertyType.IsDateTime() ||
                            x.PropertyType.IsTimeSpan())); 

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source == null ? new Dictionary<string, object>() : 
                PrimitiveProperties(source.GetType()).ToDictionary(x => 
                    x.Name, x => x.GetValue(source, null));
        }
    }
}