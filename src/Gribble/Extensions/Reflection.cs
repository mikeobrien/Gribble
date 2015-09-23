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
            var dictionary = new Dictionary<string, object>();
            if (source == null) return dictionary;
            PrimitiveProperties(source.GetType()).ToList().ForEach(x =>
            {
                try
                {
                    dictionary.Add(x.Name, x.GetValue(source, null));
                }
                catch (Exception exception)
                {
                    throw new Exception($"Error converting object {source.GetType()} " +
                        $"to dictionary. Property {x.Name} failed.", exception);
                }
            });
            return dictionary;
        }
    }
}