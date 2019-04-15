using System;
using System.Collections.Generic;
using System.Linq;

namespace Gribble.Mapping
{
    public class EntityMapping : IEntityMapping, IColumnMap, 
        IDynamicPropertyMap, IStaticPropertyMap, IEntityKey
    {
        public class InvalidMappingException : Exception
        {
            public enum MappingType { StaticColumn, StaticProperty, DynamicColumn, DynamicProperty }
            public InvalidMappingException(string name, MappingType type) :
                base($"No {type} mapping found for '{name}'.") { }
        }

        private readonly IClassMap _map;
        private readonly IEnumerable<ColumnMapping> _mappingOverride;

        public EntityMapping(IClassMap map) : this(map, null) {}

        public EntityMapping(IClassMap map, IEnumerable<ColumnMapping> mappingOverride)
        {
            _map = map;
            _mappingOverride = mappingOverride;
        }

        // ----------------- IEntityMap Implementation -----------------

        public IEntityKey Key => this;
        public IDynamicPropertyMap DynamicProperty => this;
        public IStaticPropertyMap StaticProperty => this;
        public IColumnMap Column => this;

        // ----------------- IColumnMap Implementation -----------------

        bool IColumnMap.HasStaticPropertyMapping(string columnName)
        {
            return _map.ColumPropertyMapping.ContainsKey(columnName);
        }

        string IColumnMap.GetStaticPropertyName(string columnName)
        {
            try
            {
                return _map.ColumPropertyMapping[columnName];
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidMappingException(columnName, 
                    InvalidMappingException.MappingType.StaticColumn);
            }
        }

        string IColumnMap.TryGetStaticPropertyName(string columnName)
        {
            return _map.ColumPropertyMapping.ContainsKey(columnName)
                ? _map.ColumPropertyMapping[columnName]
                : null;
        }

        bool IColumnMap.HasDynamicPropertyMapping(string columnName)
        {
            if (Key.GetColumnName() == columnName || Column.HasStaticPropertyMapping(columnName)) return false;
            return _mappingOverride == null || _mappingOverride.Any(x => x.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        string IColumnMap.GetDynamicPropertyName(string columnName)
        {
            try
            {
                return _mappingOverride == null ? columnName : _mappingOverride.First(x => x.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)).Name;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidMappingException(columnName, InvalidMappingException.MappingType.DynamicColumn);
            }
        }

        bool IColumnMap.HasPropertyMapping(string columnName)
        {
            return Column.HasStaticPropertyMapping(columnName) || Column.HasDynamicPropertyMapping(columnName);
        }

        string IColumnMap.GetPropertyName(string columnName)
        {
            return Column.HasStaticPropertyMapping(columnName) ? 
                Column.GetStaticPropertyName(columnName) : 
                Column.GetDynamicPropertyName(columnName);
        }

        // ----------------- IDynamicPropertyMap Implementation -----------------

        bool IDynamicPropertyMap.HasProperty()
        {
            return _map.HasDynamicProperty;
        }

        string IDynamicPropertyMap.GetPropertyName()
        {
            return _map.DynamicProperty;
        }

        bool IDynamicPropertyMap.HasColumnMapping(string propertyName)
        {
            if (Key.GetPropertyName() == propertyName || 
                StaticProperty.HasColumnMapping(propertyName)) return false;
            return _mappingOverride == null || 
                _mappingOverride.Any(x => x.Name == propertyName);
        }

        string IDynamicPropertyMap.GetColumnName(string propertyName)
        {
            try
            {
                return _mappingOverride == null ? propertyName : 
                    _mappingOverride.First(x => x.Name == propertyName).ColumnName;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidMappingException(propertyName, 
                    InvalidMappingException.MappingType.DynamicProperty);
            }
        }

        // ----------------- IStaticPropertyMap Implementation -----------------

        bool IStaticPropertyMap.HasColumnMapping(string propertyName)
        {
            return _map.PropertyColumMapping.ContainsKey(propertyName);
        }

        string IStaticPropertyMap.GetColumnName(string propertyName)
        {
            try
            {
                return _map.PropertyColumMapping[propertyName];
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidMappingException(propertyName, InvalidMappingException.MappingType.StaticProperty);
            }
        }

        // ----------------- IEntityKey Implementation -----------------

        public PrimaryKeyType KeyType => _map.KeyType;
        public PrimaryKeyGeneration KeyGeneration => _map.KeyGeneration;

        string IEntityKey.GetColumnName()
        {
            return _map.KeyColumn;
        }

        string IEntityKey.GetPropertyName()
        {
            return _map.KeyProperty;
        }
    }
}
