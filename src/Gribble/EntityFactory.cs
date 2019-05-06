using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gribble.Extensions;
using Gribble.Mapping;

namespace Gribble
{
    public class EntityFactory<TEntity>
    {
        private static readonly bool DefaultCtor;
        private static readonly ConstructorInfo Ctor;
        private static readonly ParameterInfo[] CtorParameters;
        private static readonly IList<PropertyInfo> Properties;

        static EntityFactory()
        {
            var entityType = typeof(TEntity);
            if (entityType.IsSimpleType()) return;
            Properties = entityType.GetProperties().ToList();
            DefaultCtor = entityType.HasDefaultCtor();
            if (DefaultCtor) return;
            Ctor = entityType.GetConstructors()[0];
            CtorParameters = Ctor.GetParameters();
        }

        public TEntity Create(IDictionary<string, object> values, IEntityMapping map)
        {
            var dynamicProperty = GetDynamicProperty(map);

            var entity = DefaultCtor
                ? CreateObjectWithDefaultCtor(values, dynamicProperty, map)
                : CreateAnonymousObject(values, dynamicProperty, map);

            if (dynamicProperty.HasProperty)
                InitDynamicValues(values, map, dynamicProperty, entity);

            return entity;
        }

        private class DynamicProperty
        {
            public PropertyInfo Property { get; set; }
            public bool HasProperty { get; set; }
            public EntityDictionary Values { get; set; }
        }

        private static DynamicProperty GetDynamicProperty(IEntityMapping map)
        {
            var property = new DynamicProperty();

            if (map.DynamicProperty.HasProperty)
            {
                property.Property = map.DynamicProperty.Property;
                property.HasProperty = property.Property != null;
                if (property.HasProperty)
                    property.Values = new EntityDictionary(map);
            }

            return property;
        }

        private TEntity CreateObjectWithDefaultCtor(IDictionary<string, object> values, 
            DynamicProperty dynamicProperty, IEntityMapping map)
        {
            var entity = Activator.CreateInstance<TEntity>();
            foreach (var property in Properties.Where(x => x.CanWrite))
            {
                MapValues(property.PropertyType, property.Name, x => property.SetValue(entity, x),
                    values, dynamicProperty, map, false);
            }
            return entity;
        }

        private TEntity CreateAnonymousObject(IDictionary<string, object> values, 
            DynamicProperty dynamicProperty, IEntityMapping map)
        {
            var parameters = new List<object>();
            foreach (var parameter in CtorParameters)
            {
                MapValues(parameter.ParameterType, parameter.Name, x => parameters.Add(x),
                    values, dynamicProperty, map, true);
            }
            return (TEntity)Ctor.Invoke(parameters.ToArray());
        }
        
        private void MapValues(Type targetType, string targetName, Action<object> mapValue, 
            IDictionary<string, object> values, DynamicProperty dynamicProperty, 
            IEntityMapping map, bool allowImplicit)
        {
            if (values.ContainsKey(targetName, x => map.Column.TryGetStaticProperty(x)?.Name))
            {
                mapValue(ConvertValue(values.Map(targetName, x =>
                    map.Column.TryGetStaticProperty(x)?.Name), targetType));
            }
            else if (allowImplicit && values.ContainsKey(targetName))
            {
                mapValue(ConvertValue(values[targetName], targetType));
            }
            else if (dynamicProperty.HasProperty && dynamicProperty.Property.Name.EqualsIgnoreCase(targetName))
            {
                mapValue(dynamicProperty.Values);
            }
        }

        private void InitDynamicValues(IDictionary<string, object> values, IEntityMapping map,
            DynamicProperty dynamicProperty, TEntity entity)
        {
            var dynamicValues = values
                .Where(x => !map.Column.HasStaticPropertyMapping(x.Key))
                .ToDictionary(x => map.Column.GetDynamicName(x.Key), 
                    x => ConvertValue(x.Value));
            dynamicProperty.Values.Init(entity, dynamicValues);
        }

        private static object ConvertValue(object value, Type type = null)
        {
            if (value == null) return null;
            type = type ?? value.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>)) return value;
            var arguments = type.GetGenericArguments();
            return arguments.Any(x => x.IsEnum) 
                ? Enum.ToObject(arguments.First(x => x.IsEnum), value) 
                : value;
        }
    }
}