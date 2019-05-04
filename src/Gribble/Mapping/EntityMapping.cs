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
        private readonly List<DynamicMapping> _dynamicMapping;
        private readonly Dictionary<string, PropertyInfo> _staticDynamicPropertyMapping;

        public EntityMapping(IClassMap map) : this(map, null) {}

        public EntityMapping(IClassMap map, IEnumerable<DynamicMapping> dynamicMapping)
        {
            _map = map;
            _dynamicMapping = dynamicMapping?.ToList();
            _staticDynamicPropertyMapping = CreateStaticDynamicPropertyMapping(_dynamicMapping);
        }

        private Dictionary<string, PropertyInfo> CreateStaticDynamicPropertyMapping(
            List<DynamicMapping> mappingOverride)
        {
            return _map.Properties
                .Select(x => new
                {
                    Name = mappingOverride?.FirstOrDefault(o => o.ColumnName
                        .EqualsIgnoreCase(x.ColumnName))?.Name ?? x.Property.Name,
                    x.Property
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
            return _map.Properties.Any(x => x.ColumnName.EqualsIgnoreCase(columnName));
        }

        PropertyInfo IColumnMap.GetStaticProperty(string columnName)
        {
            var mapping = ((IColumnMap)this).TryGetStaticProperty(columnName);
            if (mapping == null)
                throw new InvalidMappingException(columnName, 
                    InvalidMappingException.MappingType.StaticColumn);
            return mapping;
        }

        PropertyInfo IColumnMap.TryGetStaticProperty(string columnName)
        {
            return _map.Properties.FirstOrDefault(x => x
                .ColumnName.EqualsIgnoreCase(columnName))?.Property;
        }

        bool IColumnMap.HasDynamicMapping(string columnName)
        {
            return _dynamicMapping == null || _dynamicMapping.Any(
                x => x.ColumnName.EqualsIgnoreCase(columnName));
        }

        string IColumnMap.GetDynamicName(string columnName)
        {
            if (_dynamicMapping == null) return columnName;

            var dynamicName = _dynamicMapping.FirstOrDefault(x => x
                .ColumnName.EqualsIgnoreCase(columnName))?.Name;
            if (dynamicName == null)
                throw new InvalidMappingException(columnName, 
                    InvalidMappingException.MappingType.DynamicColumn);
            return dynamicName;
        }

        bool IColumnMap.HasMapping(string columnName)
        {
            return Column.HasStaticPropertyMapping(columnName) || 
                   Column.HasDynamicMapping(columnName);
        }

        string IColumnMap.GetName(string columnName)
        {
            return Column.HasStaticPropertyMapping(columnName) ? 
                Column.GetStaticProperty(columnName).Name : 
                Column.GetDynamicName(columnName);
        }

        // ----------------- IDynamicPropertyMap Implementation -----------------

        bool IDynamicPropertyMap.HasProperty => _map.DynamicProperty != null;
        PropertyInfo IDynamicPropertyMap.Property => _map.DynamicProperty;

        bool IDynamicPropertyMap.HasColumnMapping(string propertyName)
        {
            if (Key.Property?.Name == propertyName || 
                StaticProperty.HasColumnMapping(propertyName)) return false;
            return _dynamicMapping == null || 
                _dynamicMapping.Any(x => x.Name == propertyName);
        }

        string IDynamicPropertyMap.GetColumnName(string propertyName)
        {
            try
            {
                return _dynamicMapping == null ? propertyName : 
                    _dynamicMapping.First(x => x.Name == propertyName).ColumnName;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidMappingException(propertyName, 
                    InvalidMappingException.MappingType.DynamicProperty);
            }
        }

        // ----------------- IStaticPropertyMap Implementation -----------------
        
        List<PropertyMapping> IStaticPropertyMap.Mapping => _map.Properties;
        Dictionary<string, PropertyInfo> IStaticPropertyMap.StaticDynamicMapping => 
            _staticDynamicPropertyMapping;

        bool IStaticPropertyMap.HasColumnMapping(string propertyName)
        {
            return _map.Properties.Any(x => x.Property.Name.EqualsIgnoreCase(propertyName)) ||
                _staticDynamicPropertyMapping.ContainsKey(propertyName);
        }

        string IStaticPropertyMap.GetColumnName(string propertyName)
        {
            var columnName = _map.Properties.FirstOrDefault(x => x
                .Property.Name.EqualsIgnoreCase(propertyName))?.ColumnName;;
            if (columnName == null)
                throw new InvalidMappingException(propertyName, 
                    InvalidMappingException.MappingType.StaticProperty);
            return columnName;
        }

        PropertyInfo IStaticPropertyMap.GetProperty(string propertyName)
        {
            var mapping = _map.Properties.FirstOrDefault(x => x
                .Property.Name.EqualsIgnoreCase(propertyName));;
            return mapping != null
                ? mapping.Property
                : _staticDynamicPropertyMapping[propertyName];
        }

        // ----------------- IEntityKey Implementation -----------------

        public PrimaryKeyType KeyType => _map.KeyProperty.Type;
        public PrimaryKeyGeneration KeyGeneration => _map.KeyProperty.Generation;
        PropertyInfo IEntityKey.Property => _map.KeyProperty?.Property;
        string IEntityKey.ColumnName => _map.KeyProperty?.ColumnName;
    }
}
