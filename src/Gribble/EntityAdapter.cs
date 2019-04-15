using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gribble.Extensions;
using Gribble.Mapping;

namespace Gribble
{
    public class EntityAdapter<TEntity> : ILoadAdapter<TEntity>
    {
        private static IList<PropertyInfo> _properties;
        private static PropertyInfo _keyProperty;
        private static bool _hasDynamicProperty;
        private static PropertyInfo _dynamicProperty;
        private static bool _defaultCtor;
        private static ConstructorInfo _ctor;
        private static ParameterInfo[] _ctorParameters;

        private readonly IEntityMapping _map;
        private readonly IDictionary<string, object> _dynamicValues;
        private TEntity _entity;

        public EntityAdapter(IEntityMapping mapping) : 
            this(default(TEntity), mapping) { }

        public EntityAdapter(TEntity entity, IEntityMapping mapping)
        {
            if (_properties == null) Initialize(mapping);
            _entity = entity == null && _defaultCtor
                ? Activator.CreateInstance<TEntity>()
                : entity;
            if (_hasDynamicProperty) _dynamicValues = GetDynamicValues(_entity);
            _map = mapping;
        }

        public TEntity Entity => _entity;

        public object Key
        {
            get => _keyProperty.GetValue(Entity, null);
            set => _keyProperty.SetValue(Entity, value, null);
        }

        public IDictionary<string, object> GetValues()
        {
            var properties = _properties
                .Select(x => new { Name = _map.StaticProperty.GetColumnName(x.Name), Value = x.GetValue(Entity, null)});
            if (_hasDynamicProperty) properties = 
                properties
                    .Union(_dynamicValues.Where(x => _map.DynamicProperty.HasColumnMapping(x.Key))
                    .Select(x => new { Name = _map.DynamicProperty.GetColumnName(x.Key), x.Value }));
            return properties.ToDictionary(x => x.Name, x => x.Value);
        }

        public void SetValues(IDictionary<string, object> values)
        {
            if (_defaultCtor)
            {
                values.Where(x => _map.Column.HasStaticPropertyMapping(x.Key))
                   .Join(_properties, x => _map.Column.GetStaticPropertyName(x.Key), x => x.Name, 
                        (v, p) => new { v.Value, Property = p }).ToList()
                   .ForEach(x => x.Property.SetValue(Entity, ConvertValue(x.Property.PropertyType, x.Value), null));
            }
            else
            {
                var arguments = _ctorParameters
                    .Select(x => values.FirstOrDefault(y => 
                        _map.Column.TryGetStaticPropertyName(y.Key) == x.Name || 
                        y.Key.EqualsIgnoreCase(x.Name)).Value)
                    .ToArray();
                _entity = (TEntity)_ctor.Invoke(arguments);
            }

            if (_hasDynamicProperty) values.Where(x => _map.Column.HasDynamicPropertyMapping(x.Key))
                .Select(x => new { Name = _map.Column.GetDynamicPropertyName(x.Key), x.Value })
                .ToList().ForEach(x =>
                        {
                            if (_dynamicValues.ContainsKey(x.Name)) _dynamicValues[x.Name] = x.Value;
                            else _dynamicValues.Add(x.Name, x.Value);
                        });
        }

        private static object ConvertValue(Type type, object value)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null) return null;
                var arguments = type.GetGenericArguments();
                if (arguments.Any(x => x.IsEnum)) return Enum.ToObject(arguments.First(x => x.IsEnum), value);
            }
            return value;
        }

        private static void Initialize(IEntityMapping map)
        {
            string dynamicPropertyName = null;

            _defaultCtor = typeof(TEntity).HasDefaultCtor();

            if (!_defaultCtor)
            {
                _ctor = typeof(TEntity).GetConstructors()[0];
                _ctorParameters = _ctor.GetParameters();
            }
            else
            {
                _hasDynamicProperty = map.DynamicProperty.HasProperty();
                if (_hasDynamicProperty)
                {
                    dynamicPropertyName = map.DynamicProperty.GetPropertyName();
                    _dynamicProperty = typeof(TEntity).GetProperties().First(x => x.CanRead && x.CanWrite && x.Name == dynamicPropertyName);
                }
            }

            _properties = typeof(TEntity).GetProperties()
                .Where(x => x.CanRead && x.CanWrite && 
                            map.StaticProperty.HasColumnMapping(x.Name) && 
                            x.Name != dynamicPropertyName).ToList();
            _keyProperty = _properties.FirstOrDefault(x => x.Name == map.Key.GetPropertyName());
        }

        private static Dictionary<string, object> GetDynamicValues(TEntity entity)
        {
            var values = (Dictionary<string, object>)_dynamicProperty.GetValue(entity, null);
            if (values == null)
            {
                values = new Dictionary<string, object>();
                _dynamicProperty.SetValue(entity, values, null);
            }
            return values;
        }
    }
}
