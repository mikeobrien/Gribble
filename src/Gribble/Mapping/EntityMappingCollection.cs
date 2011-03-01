using System;
using System.Collections.Generic;

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
                    throw new Exception(string.Format("Duplicate class mapping found for type {0}.", classMap.Type.Name));
                _classMappings.Add(classMap.Type, classMap);
            }
        }

        public IEntityMapping GetEntityMapping<TEntity>()
        {
            return new EntityMapping(_classMappings[typeof(TEntity)]);
        }

        public IEntityMapping GetEntityMapping<TEntity>(IEnumerable<ColumnMapping> mappingOverride)
        {
            return new EntityMapping(_classMappings[typeof(TEntity)], mappingOverride);
        }
    }
}
