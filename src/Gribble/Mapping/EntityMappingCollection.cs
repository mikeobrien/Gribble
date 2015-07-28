using System;
using System.Collections.Generic;
using Gribble.Extensions;

namespace Gribble.Mapping
{
    public class EntityMappingCollection
    {
        private readonly IDictionary<Type, IClassMap> _classMappings = new Dictionary<Type, IClassMap>();

        public EntityMappingCollection(IEnumerable<IClassMap> classMappings)
        {
            foreach (var classMap in classMappings)
            {
                if (_classMappings.ContainsKey(classMap.Type)) 
                    throw new Exception($"Duplicate class mapping found for type {classMap.Type.Name}.");
                _classMappings.Add(classMap.Type, classMap);
            }
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
            return _classMappings.ContainsKey(type)
                ? _classMappings[type]
                : _classMappings.AddItem(type, new AutoClassMap<TEntity>());
        }
    }
}
