using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gribble.Mapping;

namespace Gribble
{
    public class EntityAdapter<TEntity>
    {
        private static IList<PropertyInfo> _properties;
        private static PropertyInfo _keyProperty;
        private static bool _hasDynamicProperty;
        private static PropertyInfo _dynamicProperty;

        private readonly IEntityMapping _map;
        private readonly TEntity _entity;

        public EntityAdapter(TEntity entity, IEntityMapping mapping)
        {
            _map = mapping;
            _entity = entity;
            if (_properties == null) Initialize(mapping);
        }

        public object Key
        {
            get => _keyProperty.GetValue(_entity, null);
            set => _keyProperty.SetValue(_entity, value, null);
        }

        public IDictionary<string, object> GetValues()
        {
            var properties = _properties
                .Select(x => new
                {
                    Name = _map.StaticProperty.GetColumnName(x.Name),
                    Value = x.GetValue(_entity, null)
                });

            if (_hasDynamicProperty) 
                properties = properties
                    .Union(TryGetDynamicValues().Where(x => !_map.StaticProperty.HasColumnMapping(x.Key))
                    .Select(x => new
                    {
                        Name = _map.DynamicProperty.GetColumnName(x.Key),
                        x.Value
                    }));

            return properties.ToDictionary(x => x.Name, x => x.Value);
        }

        private static void Initialize(IEntityMapping map)
        {
            string dynamicPropertyName = null;
            
            _hasDynamicProperty = map.DynamicProperty.HasProperty();

            if (_hasDynamicProperty)
            {
                dynamicPropertyName = map.DynamicProperty.GetProperty().Name;
                _dynamicProperty = typeof(TEntity).GetProperties()
                    .First(x => x.CanRead && x.CanWrite && x.Name == dynamicPropertyName);
            }

            _properties = typeof(TEntity).GetProperties()
                .Where(x => x.CanRead && x.CanWrite && 
                            map.StaticProperty.HasColumnMapping(x.Name) && 
                            x.Name != dynamicPropertyName).ToList();
            _keyProperty = map.Key.GetProperty();
        }

        private IDictionary<string, object> TryGetDynamicValues()
        {
            var values = (IDictionary<string, object>)_dynamicProperty.GetValue(_entity, null);

            if (values is EntityDictionary)
                values = ((EntityDictionary) values).DynamicValues;

            return values;
        }
    }
}
