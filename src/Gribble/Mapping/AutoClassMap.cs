using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gribble.Mapping
{
    public class AutoClassMap<T> : ClassMap<T>
    {
        private static readonly Type DictionaryType = typeof(IDictionary<string, object>);
        public AutoClassMap()
        {
            var type = typeof(T);
            var properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            var dynamicProperty = properties.FirstOrDefault(x => 
                DictionaryType.IsAssignableFrom(x.PropertyType));
            if (dynamicProperty != null) MapDynamic(dynamicProperty.Name);

            var idProperty = properties.FirstOrDefault(x =>
                x.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                (x.PropertyType == typeof(int) || x.PropertyType == typeof(Guid)));
            if (idProperty != null) Id(idProperty.Name, idProperty.PropertyType);

            properties.Where(x => x != dynamicProperty && x != idProperty)
                .ToList().ForEach(x => Map(x.Name));
        } 
    }
}
