using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gribble.Extensions;

namespace Gribble.Mapping
{
    public class EntityMapping : IEntityMapping, IColumnMap, 
        IDynamicPropertyMap, IStaticPropertyMap, IEntityKey
    {
        public class InvalidMappingException : Exception
        {
            public enum MappingType
            {
                StaticColumn,
                StaticProperty,
                DynamicColumn,
                DynamicProperty
            }

            public InvalidMappingException(string name, MappingType type) :
                base($"No {type} mapping found for '{name}'.") { }
        }

        private readonly IClassMap _map;
        private readonly List<ColumnMapping> _mappingOverride;
        private readonly Dictionary<string, PropertyInfo> _staticDynamicPropertyMapping;

        public EntityMapping(IClassMap map) : this(map, null) {}

        public EntityMapping(IClassMap map, IEnumerable<ColumnMapping> mappingOverride)
        {
            _map = map;
            _mappingOverride = mappingOverride?.ToList();
            _staticDynamicPropertyMapping = CreateStaticDynamicPropertyMapping(_mappingOverride);
        }

        private Dictionary<string, PropertyInfo> CreateStaticDynamicPropertyMapping(
            List<ColumnMapping> mappingOverride)
        {
            return _map.ColumPropertyMapping
                .Select(x => new
                {
                    Name = mappingOverride?.FirstOrDefault(o => o.ColumnName
                        .EqualsIgnoreCase(x.Key))?.Name ?? x.Value.Name,
                    Property = x.Value
                })
                .ToDictionary(x => x.Name, x => x.Property, StringComparer.OrdinalIgnoreCase);
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

        PropertyInfo IColumnMap.GetStaticProperty(string columnName)
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

        PropertyInfo IColumnMap.TryGetStaticProperty(string columnName)
        {
            return _map.ColumPropertyMapping.ContainsKey(columnName)
                ? _map.ColumPropertyMapping[columnName]
                : null;
        }

        bool IColumnMap.HasDynamicPropertyMapping(string columnName)
        {
            return _mappingOverride == null || _mappingOverride.Any(x => x.ColumnName.EqualsIgnoreCase(columnName));
        }

        string IColumnMap.GetDynamicPropertyName(string columnName)
        {
            try
            {
                return _mappingOverride == null 
                    ? columnName 
                    : _mappingOverride.First(x => x.ColumnName.EqualsIgnoreCase(columnName)).Name;
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
                Column.GetStaticProperty(columnName).Name : 
                Column.GetDynamicPropertyName(columnName);
        }

        // ----------------- IDynamicPropertyMap Implementation -----------------

        bool IDynamicPropertyMap.HasProperty()
        {
            return _map.HasDynamicProperty;
        }

        PropertyInfo IDynamicPropertyMap.GetProperty()
        {
            return _map.DynamicProperty;
        }

        bool IDynamicPropertyMap.HasColumnMapping(string propertyName)
        {
            if (Key.GetProperty()?.Name == propertyName || 
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
        
        List<PropertyInfo> IStaticPropertyMap.Properties => _map.Properties;
        Dictionary<string, PropertyInfo> IStaticPropertyMap.StaticDynamicMapping => _staticDynamicPropertyMapping;

        bool IStaticPropertyMap.HasColumnMapping(string propertyName)
        {
            return _map.PropertyColumMapping.ContainsKey(propertyName) ||
                _staticDynamicPropertyMapping.ContainsKey(propertyName);
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

        PropertyInfo IStaticPropertyMap.GetProperty(string propertyName)
        {
            return _map.PropertyColumMapping.ContainsKey(propertyName)
                ? _map.PropertyNameMapping[propertyName]
                : _staticDynamicPropertyMapping[propertyName];
        }

        // ----------------- IEntityKey Implementation -----------------

        public PrimaryKeyType KeyType => _map.KeyType;
        public PrimaryKeyGeneration KeyGeneration => _map.KeyGeneration;

        string IEntityKey.GetColumnName()
        {
            return _map.KeyColumn;
        }

        PropertyInfo IEntityKey.GetProperty()
        {
            return _map.KeyProperty;
        }
    }
}
