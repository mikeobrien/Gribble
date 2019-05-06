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
                Column,
                Property
            }

            public InvalidMappingException(string name, MappingType type) :
                base($"No {type} mapping found for '{name}'.") { }
        }

        private readonly IClassMap _map;
        private readonly bool _hasDynamicMapping;
        private readonly List<DynamicMapping> _dynamicMapping;
        private readonly Dictionary<string, PropertyInfo> _staticDynamicPropertyMapping;

        public EntityMapping(IClassMap map) : this(map, null) {}

        public EntityMapping(IClassMap map, IEnumerable<DynamicMapping> dynamicMapping)
        {
            _map = map;
            _dynamicMapping = dynamicMapping.UnionOrDefault(map.DynamicProperty?.Mapping)?.ToList();
            _hasDynamicMapping = _dynamicMapping != null && _dynamicMapping.Any();
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
                    InvalidMappingException.MappingType.Column);
            return mapping;
        }

        PropertyInfo IColumnMap.TryGetStaticProperty(string columnName)
        {
            return _map.Properties.FirstOrDefault(x => x
                .ColumnName.EqualsIgnoreCase(columnName))?.Property;
        }

        bool IColumnMap.HasDynamicMapping(string columnName)
        {
            return !_hasDynamicMapping || _dynamicMapping.Any(
                x => x.ColumnName.EqualsIgnoreCase(columnName));
        }

        string IColumnMap.GetDynamicName(string columnName)
        {
            var dynamicName = _dynamicMapping?.FirstOrDefault(x => x
                .ColumnName.EqualsIgnoreCase(columnName))?.Name;
            return dynamicName ?? columnName;
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

        bool IDynamicPropertyMap.HasProperty => _map.DynamicProperty?.Property != null;
        PropertyInfo IDynamicPropertyMap.Property => _map.DynamicProperty.Property;

        bool IDynamicPropertyMap.HasColumnMapping(string propertyName)
        {
            if ((Key.Property?.Name).EqualsIgnoreCase(propertyName) || 
                StaticProperty.HasColumnMapping(propertyName)) return false;
            return !_hasDynamicMapping || 
                _dynamicMapping.Any(x => x.Name.EqualsIgnoreCase(propertyName));
        }

        bool IDynamicPropertyMap.IsReadonly(string propertyName)
        {
            return _dynamicMapping?.FirstOrDefault(x => x.Name
                .EqualsIgnoreCase(propertyName))?.Readonly ?? false;
        }

        string IDynamicPropertyMap.GetColumnName(string propertyName)
        {
            var columnName = _dynamicMapping?.FirstOrDefault(x => x.Name
                .EqualsIgnoreCase(propertyName))?.ColumnName;
            return columnName ?? propertyName;
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
                    InvalidMappingException.MappingType.Property);
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
        
        public bool HasKey => _map.KeyProperty != null;
        public PrimaryKeyType KeyType => _map.KeyProperty.Type;
        public PrimaryKeyGeneration KeyGeneration => _map.KeyProperty.Generation;
        PropertyInfo IEntityKey.Property => _map.KeyProperty?.Property;
        string IEntityKey.ColumnName => _map.KeyProperty?.ColumnName;
    }
}
