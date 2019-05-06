using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;

namespace Gribble
{
    public class EntityAdapter<TEntity>
    {
        private readonly IEntityMapping _map;
        private readonly TEntity _entity;

        public EntityAdapter(TEntity entity, IEntityMapping mapping)
        {
            _map = mapping;
            _entity = entity;
        }

        public object Key
        {
            get => _map.Key.Property.GetValue(_entity, null);
            set => _map.Key.Property.SetValue(_entity, value, null);
        }

        public IDictionary<string, object> GetValues()
        {
            var properties = _map.StaticProperty.Mapping
                .Where(x => !x.Readonly)
                .Select(x => new
                {
                    Name = x.ColumnName,
                    Value = x.Property.GetValue(_entity, null)
                });

            if (_map.DynamicProperty.HasProperty) 
                properties = properties
                    .Union(TryGetDynamicValues()
                        .Where(x => !_map.DynamicProperty.IsReadonly(x.Key) && 
                                    !_map.StaticProperty.HasColumnMapping(x.Key))
                    .Select(x => new
                    {
                        Name = _map.DynamicProperty.GetColumnName(x.Key),
                        x.Value
                    }));

            return properties.ToDictionary(x => x.Name, x => x.Value);
        }

        private IDictionary<string, object> TryGetDynamicValues()
        {
            var values = (IDictionary<string, object>)_map
                .DynamicProperty.Property.GetValue(_entity, null);

            if (values is EntityDictionary)
                values = ((EntityDictionary) values).DynamicValues;

            return values;
        }
    }
}
