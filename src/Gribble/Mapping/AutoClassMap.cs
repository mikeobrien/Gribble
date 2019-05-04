using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gribble.Extensions;

namespace Gribble.Mapping
{
    public class AutoClassMap<T> : ClassMap<T>
    {
        public AutoClassMap(string keyName = null)
        {
            var type = typeof(T);
            var properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            var dynamicProperty = properties.FirstOrDefault(x => x.PropertyType.IsGenericType && 
                x.PropertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (dynamicProperty != null) MapDynamic(dynamicProperty);

            var idProperty = properties.FirstOrDefault(x =>
                x.Name.Equals(keyName ?? "Id", StringComparison.OrdinalIgnoreCase) &&
                (x.PropertyType.IsInteger() || x.PropertyType.IsGuid() || 
                 x.PropertyType.IsString()));
            if (idProperty != null)
            {
                Id(idProperty, idProperty.PropertyType);
                if (idProperty.PropertyType.IsInteger())
                    KeyProperty.Generation = PrimaryKeyGeneration.Server;
                if (idProperty.PropertyType.IsGuid())
                    KeyProperty.Generation = PrimaryKeyGeneration.Client;
            }

            properties.Where(x => x != dynamicProperty && x != idProperty)
                .ToList().ForEach(Map);
        } 

        protected void Id(PropertyInfo property, Type type)
        {
            PrimaryKeyType keyType;

            if (type.IsInteger()) keyType = PrimaryKeyType.Integer;
            else if (type == typeof(Guid)) keyType = PrimaryKeyType.Guid;
            else if (type == typeof(string)) keyType = PrimaryKeyType.String;
            else throw new ArgumentException("Type must be an integer, guid or string.");

            new KeyMappingDsl(property, keyType, this).Column(property.Name);
        }
    }
}
