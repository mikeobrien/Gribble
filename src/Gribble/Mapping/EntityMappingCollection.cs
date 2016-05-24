using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Gribble.Mapping
{
    public class EntityMappingCollection
    {
        private readonly ConcurrentDictionary<Type, IClassMap> _classMappings = new ConcurrentDictionary<Type, IClassMap>();

        public EntityMappingCollection(IEnumerable<IClassMap> classMappings)
        {
            foreach (var classMap in classMappings)
            {
                if (_classMappings.ContainsKey(classMap.Type)) 
                    throw new Exception($"Duplicate class mapping found for type {classMap.Type.Name}.");
                _classMappings.TryAdd(classMap.Type, classMap);
            }
        }

        public static EntityMappingCollection Empty()
        {
            return new EntityMappingCollection(Enumerable.Empty<IClassMap>());
        }

        public IEntityMapping GetEntityMapping<TEntity>()
        {
            return new EntityMapping(GetMappingOrDefault<TEntity>());
        }

        public IEntityMapping GetEntityMapping<TEntity>(IEnumerable<ColumnMapping> mappingOverride)
        {
            return new EntityMapping(GetMappingOrDefault<TEntity>(), mappingOverride);
        }

        private IClassMap GetMappingOrDefault<TEntity>()
        {
            var type = typeof (TEntity);
            if (!_classMappings.ContainsKey(type))
                _classMappings.TryAdd(type, new AutoClassMap<TEntity>());
            return _classMappings[type];
        }
    }
}
