using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gribble.Mapping
{
    public abstract class ClassMap<T> : IClassMap
    {
        private readonly Dictionary<string, string> _propertyColumnMapping = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _columnPropertyMapping = new Dictionary<string, string>();

        public IDictionary<string, string> PropertyColumMapping { get { return _propertyColumnMapping; } }
        public IDictionary<string, string> ColumPropertyMapping { get { return _columnPropertyMapping; } }

        public string DynamicProperty { get; set; }
        public bool HasDynamicProperty { get { return !string.IsNullOrEmpty(DynamicProperty); } }

        public PrimaryKeyType KeyType { get; set; }
        public string KeyColumn { get; set; }
        public string KeyProperty { get; set; }

        public Type Type { get { return typeof(T); } }

        public IdentityKeyMapping Id(Expression<Func<T, int>> property)
        {
            return new IdentityKeyMapping(GetPropertyName(property), this);
        }

        public GuidKeyMapping Id(Expression<Func<T, Guid>> property)
        {
            return new GuidKeyMapping(GetPropertyName(property), this);
        }

        public ColumnMapping Map<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return new ColumnMapping(GetPropertyName(property), this);
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
            if (_propertyColumnMapping.ContainsKey(propertyName)) _propertyColumnMapping[propertyName] = columnName;
            else _propertyColumnMapping.Add(propertyName, columnName);

            if (_columnPropertyMapping.ContainsKey(columnName)) _columnPropertyMapping[columnName] = propertyName;
            else _columnPropertyMapping.Add(columnName, propertyName);
        }

        private void AddKeyMapping(PrimaryKeyType type, string columnName, string propertyName)
        {
            AddMapping(columnName, propertyName);
            KeyType = type;
            KeyColumn = columnName;
            KeyProperty = propertyName;
        }

        private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> property)
        {
            if (property.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Mapping must be a property.", "property");
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

        public class IdentityKeyMapping : Mapping
        {
            public IdentityKeyMapping(string propertyName, ClassMap<T> map) : base(propertyName, map)
            {
                Column(propertyName);
            }

            public void Column(string name)
            { Map.AddKeyMapping(PrimaryKeyType.IdentitySeed, name, PropertyName); }
        }

        public class GuidKeyMapping : Mapping
        {
            public GuidKeyMapping(string propertyName, ClassMap<T> map) : base(propertyName, map)
            {
                Column(propertyName);
            }

            public void Generated()
            {
                Map.KeyType = PrimaryKeyType.GuidClientGenerated;
            }

            public GuidKeyMapping Column(string name)
            {
                Map.AddKeyMapping(PrimaryKeyType.GuidServerGenerated, name, PropertyName);
                return this;
            }
        }
    }
}
