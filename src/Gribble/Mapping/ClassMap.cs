using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gribble.Extensions;

namespace Gribble.Mapping
{
    public abstract class ClassMap<T> : IClassMap
    {
        private readonly Dictionary<string, string> _propertyColumnMapping = new Dictionary<string, string>();
        private readonly Dictionary<string, PropertyInfo> _propertyNameMapping = 
            new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PropertyInfo> _columnPropertyMapping = 
            new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly List<PropertyInfo> _properties = new List<PropertyInfo>();

        public IDictionary<string, string> PropertyColumMapping => _propertyColumnMapping;
        public IDictionary<string, PropertyInfo> ColumPropertyMapping => _columnPropertyMapping;
        public IDictionary<string, PropertyInfo> PropertyNameMapping => _propertyNameMapping;
        public List<PropertyInfo> Properties => _properties;

        public PropertyInfo DynamicProperty { get; set; }
        public bool HasDynamicProperty => DynamicProperty != null;

        public PrimaryKeyType KeyType { get; set; }
        public PrimaryKeyGeneration KeyGeneration { get; set; }
        public string KeyColumn { get; set; }
        public PropertyInfo KeyProperty { get; set; }

        public Type Type => typeof(T);

        public void Id(PropertyInfo property, Type type)
        {
            PrimaryKeyType keyType;

            if (type.IsInteger()) keyType = PrimaryKeyType.Integer;
            else if (type == typeof(Guid)) keyType = PrimaryKeyType.Guid;
            else if (type == typeof(string)) keyType = PrimaryKeyType.String;
            else throw new ArgumentException("Type must be an integer, guid or string.");

            new KeyMapping(property, keyType, this).Column(property.Name);
        }

        public IntegerKeyMapping Id(Expression<Func<T, int>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMapping Id(Expression<Func<T, uint>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMapping Id(Expression<Func<T, long>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMapping Id(Expression<Func<T, ulong>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMapping Id(Expression<Func<T, decimal>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMapping Id(Expression<Func<T, float>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMapping Id(Expression<Func<T, short>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMapping Id(Expression<Func<T, ushort>> property)
        {
            return IntegerId(property.GetProperty());
        }

        private IntegerKeyMapping IntegerId(PropertyInfo property)
        {
            return new IntegerKeyMapping(property, this).Column(property.Name);
        }

        public GuidKeyMapping Id(Expression<Func<T, Guid>> property)
        {
            var propertyInfo = property.GetProperty();
            return new GuidKeyMapping(propertyInfo, this).Column(propertyInfo.Name);
        }

        public KeyMapping Id(Expression<Func<T, string>> property)
        {
            var propertyInfo = property.GetProperty();
            return new KeyMapping(propertyInfo, PrimaryKeyType.String, this).Column(propertyInfo.Name);
        }

        public void Map(PropertyInfo property)
        {
            new ColumnMapping(property, this).Column(property.Name);
        }

        public ColumnMapping Map<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return new ColumnMapping(property.GetProperty(), this);
        }

        public void MapDynamic(PropertyInfo property)
        {
            new DynamicMapping(property, this).Dynamic();
        }

        public DynamicMapping Map(Expression<Func<T, IDictionary<string, object>>> property)
        {
            return new DynamicMapping(property.GetProperty(), this);
        }

        private void AddKeyMapping(PrimaryKeyType type, string columnName, PropertyInfo property)
        {
            AddMapping(columnName, property);
            KeyType = type;
            KeyGeneration = PrimaryKeyGeneration.None;
            KeyColumn = columnName;
            KeyProperty = property;
        }

        private void AddMapping(string columnName, PropertyInfo property)
        {
            _propertyColumnMapping.Where(x => x.Value == columnName).ToList()
                .ForEach(x => _propertyColumnMapping.Remove(x.Key));
            _columnPropertyMapping.Where(x => x.Value == property).ToList()
                .ForEach(x =>
                {
                    _columnPropertyMapping.Remove(x.Key);
                    if (_properties.Contains(property))
                        _properties.Remove(property);
                });

            _propertyColumnMapping[property.Name] = columnName;
            _columnPropertyMapping[columnName] = property;
            _propertyNameMapping[property.Name] = property;

            if (!_properties.Contains(property))
                _properties.Add(property);
        }

        public abstract class Mapping
        {
            protected readonly ClassMap<T> Map;
            protected readonly PropertyInfo Property;

            protected Mapping(PropertyInfo property, ClassMap<T> map)
            {
                Map = map;
                Property = property;
            }
        }

        public class ColumnMapping : Mapping
        {
            public ColumnMapping(PropertyInfo property, ClassMap<T> map) : base(property, map)
            {
                Column(property.Name);
            }
            public void Column(string name) { Map.AddMapping(name, Property); }
        }

        public class DynamicMapping : Mapping
        {
            public DynamicMapping(PropertyInfo property, ClassMap<T> map) : base(property, map) { }
            public void Dynamic() { Map.DynamicProperty = Property; }
        }

        public class KeyMapping : Mapping
        {
            private readonly PrimaryKeyType _type;

            public KeyMapping(PropertyInfo property, PrimaryKeyType type, ClassMap<T> map) : 
                base(property, map)
            {
                _type = type;
            }

            public virtual KeyMapping Column(string name)
            {
                Map.AddKeyMapping(_type, name, Property);
                return this;
            }
        }

        public class IntegerKeyMapping : KeyMapping
        {
            public IntegerKeyMapping(PropertyInfo property, ClassMap<T> map) :
                base(property, PrimaryKeyType.Integer, map) { }

            public void Identity()
            {
                Map.KeyGeneration = PrimaryKeyGeneration.Server;
            }

            public new IntegerKeyMapping Column(string name)
            {
                base.Column(name);
                return this;
            }
        }

        public class GuidKeyMapping : KeyMapping
        {
            public GuidKeyMapping(PropertyInfo property, ClassMap<T> map) :
                base(property, PrimaryKeyType.Guid, map) { }

            public void GuidComb()
            {
                Map.KeyGeneration = PrimaryKeyGeneration.Client;
            }

            public new GuidKeyMapping Column(string name)
            {
                base.Column(name);
                return this;
            }
        }
    }
}
