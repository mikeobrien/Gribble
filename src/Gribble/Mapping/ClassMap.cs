using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gribble.Extensions;

namespace Gribble.Mapping
{
    public abstract class ClassMap<T> : IClassMap
    {
        private readonly Dictionary<string, string> _propertyColumnMapping = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _columnPropertyMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> PropertyColumMapping => _propertyColumnMapping;
        public IDictionary<string, string> ColumPropertyMapping => _columnPropertyMapping;

        public string DynamicProperty { get; set; }
        public bool HasDynamicProperty => !string.IsNullOrEmpty(DynamicProperty);

        public PrimaryKeyType KeyType { get; set; }
        public PrimaryKeyGeneration KeyGeneration { get; set; }
        public string KeyColumn { get; set; }
        public string KeyProperty { get; set; }

        public Type Type => typeof(T);

        public void Id(string name, Type type)
        {
            PrimaryKeyType keyType;

            if (type.IsInteger()) keyType = PrimaryKeyType.Integer;
            else if (type == typeof(Guid)) keyType = PrimaryKeyType.Guid;
            else if (type == typeof(string)) keyType = PrimaryKeyType.String;
            else throw new ArgumentException("Type must be an integer, guid or string.");

            new KeyMapping(name, keyType, this).Column(name);
        }

        public IntegerKeyMapping Id(Expression<Func<T, int>> property)
        {
            return IntegerId(GetPropertyName(property));
        }

        public IntegerKeyMapping Id(Expression<Func<T, uint>> property)
        {
            return IntegerId(GetPropertyName(property));
        }

        public IntegerKeyMapping Id(Expression<Func<T, long>> property)
        {
            return IntegerId(GetPropertyName(property));
        }

        public IntegerKeyMapping Id(Expression<Func<T, ulong>> property)
        {
            return IntegerId(GetPropertyName(property));
        }

        public IntegerKeyMapping Id(Expression<Func<T, decimal>> property)
        {
            return IntegerId(GetPropertyName(property));
        }

        public IntegerKeyMapping Id(Expression<Func<T, float>> property)
        {
            return IntegerId(GetPropertyName(property));
        }

        public IntegerKeyMapping Id(Expression<Func<T, short>> property)
        {
            return IntegerId(GetPropertyName(property));
        }

        public IntegerKeyMapping Id(Expression<Func<T, ushort>> property)
        {
            return IntegerId(GetPropertyName(property));
        }

        private IntegerKeyMapping IntegerId(string name)
        {
            return new IntegerKeyMapping(name, this).Column(name);
        }

        public GuidKeyMapping Id(Expression<Func<T, Guid>> property)
        {
            var name = GetPropertyName(property);
            return new GuidKeyMapping(name, this).Column(name);
        }

        public KeyMapping Id(Expression<Func<T, string>> property)
        {
            var name = GetPropertyName(property);
            return new KeyMapping(name, PrimaryKeyType.String, this).Column(name);
        }

        public void Map(string name)
        {
            new ColumnMapping(name, this).Column(name);
        }

        public ColumnMapping Map<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return new ColumnMapping(GetPropertyName(property), this);
        }

        public void MapDynamic(string propertyName)
        {
            new DynamicMapping(propertyName, this).Dynamic();
        }

        public DynamicMapping Map(Expression<Func<T, Dictionary<string, object>>> property)
        {
            return new DynamicMapping(GetPropertyName(property), this);
        }

        public DynamicMapping Map(Expression<Func<T, IDictionary<string, object>>> property)
        {
            return new DynamicMapping(GetPropertyName(property), this);
        }

        private void AddMapping(string columnName, string propertyName)
        {
            _propertyColumnMapping.Where(x => x.Value == columnName).ToList()
                .ForEach(x => _propertyColumnMapping.Remove(x.Key));
            _columnPropertyMapping.Where(x => x.Value == propertyName).ToList()
                .ForEach(x => _columnPropertyMapping.Remove(x.Key));

            _propertyColumnMapping[propertyName] = columnName;
            _columnPropertyMapping[columnName] = propertyName;
        }

        private void AddKeyMapping(PrimaryKeyType type, string columnName, string propertyName)
        {
            AddMapping(columnName, propertyName);
            KeyType = type;
            KeyGeneration = PrimaryKeyGeneration.None;
            KeyColumn = columnName;
            KeyProperty = propertyName;
        }

        private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Mapping must be a property.", nameof(property));
            return ((MemberExpression)property.Body).Member.Name;
        }

        public abstract class Mapping
        {
            protected readonly ClassMap<T> Map;
            protected readonly string PropertyName;

            protected Mapping(string propertyName, ClassMap<T> map)
            {
                Map = map;
                PropertyName = propertyName;
            }
        }

        public class ColumnMapping : Mapping
        {
            public ColumnMapping(string propertyName, ClassMap<T> map) : base(propertyName, map)
            {
                Column(propertyName);
            }
            public void Column(string name) { Map.AddMapping(name, PropertyName); }
        }

        public class DynamicMapping : Mapping
        {
            public DynamicMapping(string propertyName, ClassMap<T> map) : base(propertyName, map) { }
            public void Dynamic() { Map.DynamicProperty = PropertyName; }
        }

        public class KeyMapping : Mapping
        {
            private readonly PrimaryKeyType _type;

            public KeyMapping(string propertyName, PrimaryKeyType type, ClassMap<T> map) : 
                base(propertyName, map)
            {
                _type = type;
            }

            public virtual KeyMapping Column(string name)
            {
                Map.AddKeyMapping(_type, name, PropertyName);
                return this;
            }
        }

        public class IntegerKeyMapping : KeyMapping
        {
            public IntegerKeyMapping(string propertyName, ClassMap<T> map) :
                base(propertyName, PrimaryKeyType.Integer, map) { }

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
            public GuidKeyMapping(string propertyName, ClassMap<T> map) :
                base(propertyName, PrimaryKeyType.Guid, map) { }

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
