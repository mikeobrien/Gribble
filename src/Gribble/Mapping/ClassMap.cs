using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Gribble.Extensions;

namespace Gribble.Mapping
{
    public abstract class ClassMap<T> : IClassMap
    {
        public List<PropertyMapping> Properties { get; } = new List<PropertyMapping>();
        public PropertyInfo DynamicProperty { get; set; }
        public KeyPropertyMapping KeyProperty { get; set; }

        public Type Type => typeof(T);

        public IntegerKeyMappingDsl Id(Expression<Func<T, int>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMappingDsl Id(Expression<Func<T, uint>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMappingDsl Id(Expression<Func<T, long>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMappingDsl Id(Expression<Func<T, ulong>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMappingDsl Id(Expression<Func<T, decimal>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMappingDsl Id(Expression<Func<T, float>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMappingDsl Id(Expression<Func<T, short>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public IntegerKeyMappingDsl Id(Expression<Func<T, ushort>> property)
        {
            return IntegerId(property.GetProperty());
        }

        public GuidKeyMappingDsl Id(Expression<Func<T, Guid>> property)
        {
            var propertyInfo = property.GetProperty();
            return new GuidKeyMappingDsl(propertyInfo, this).Column(propertyInfo.Name);
        }

        public KeyMappingDsl Id(Expression<Func<T, string>> property)
        {
            var propertyInfo = property.GetProperty();
            return new KeyMappingDsl(propertyInfo, PrimaryKeyType.String, this).Column(propertyInfo.Name);
        }

        public ColumnMappingDsl Map<TProperty>(Expression<Func<T, TProperty>> property)
        {
            return new ColumnMappingDsl(property.GetProperty(), this);
        }

        public DynamicMappingDsl Map(Expression<Func<T, IDictionary<string, object>>> property)
        {
            return new DynamicMappingDsl(property.GetProperty(), this);
        }

        protected IntegerKeyMappingDsl IntegerId(PropertyInfo property)
        {
            return new IntegerKeyMappingDsl(property, this).Column(property.Name);
        }

        protected void Map(PropertyInfo property)
        {
            new ColumnMappingDsl(property, this).Column(property.Name);
        }

        protected void MapDynamic(PropertyInfo property)
        {
            new DynamicMappingDsl(property, this).Dynamic();
        }

        private void AddKeyMapping(PrimaryKeyType type, string columnName, PropertyInfo property)
        {
            AddMapping(KeyProperty = new KeyPropertyMapping(columnName, property, type));
        }

        private PropertyMapping AddMapping(string columnName, PropertyInfo property)
        {
            return AddMapping(new PropertyMapping(columnName, property));
        }

        private PropertyMapping AddMapping(PropertyMapping mapping)
        {
            Properties.RemoveAll(x => x.Property == mapping.Property);
            Properties.Add(mapping);
            return mapping;
        }

        public abstract class MappingDslBase
        {
            protected readonly ClassMap<T> Map;
            protected readonly PropertyInfo Property;

            protected MappingDslBase(PropertyInfo property, ClassMap<T> map)
            {
                Map = map;
                Property = property;
            }
        }

        public class ColumnMappingDsl : MappingDslBase
        {
            public ColumnMappingDsl(PropertyInfo property, ClassMap<T> map) : base(property, map)
            {
                Column(property.Name);
            }

            public MappingConfigDsl Column(string name)
            {
                return new MappingConfigDsl(Map.AddMapping(name, Property));
            }
        }

        public class MappingConfigDsl
        {
            private readonly PropertyMapping _propertyMapping;

            public MappingConfigDsl(PropertyMapping propertyMapping)
            {
                _propertyMapping = propertyMapping;
            }

            public void Readonly()
            {
                _propertyMapping.Readonly = true;
            }
        }

        public class DynamicMappingDsl : MappingDslBase
        {
            public DynamicMappingDsl(PropertyInfo property, ClassMap<T> map) : base(property, map) { }

            public void Dynamic()
            {
                Map.DynamicProperty = Property;
            }
        }

        public class KeyMappingDsl : MappingDslBase
        {
            private readonly PrimaryKeyType _type;

            public KeyMappingDsl(PropertyInfo property, PrimaryKeyType type, ClassMap<T> map) : 
                base(property, map)
            {
                _type = type;
            }

            public virtual KeyMappingDsl Column(string name)
            {
                Map.AddKeyMapping(_type, name, Property);
                return this;
            }
        }

        public class IntegerKeyMappingDsl : KeyMappingDsl
        {
            public IntegerKeyMappingDsl(PropertyInfo property, ClassMap<T> map) :
                base(property, PrimaryKeyType.Integer, map) { }

            public void Identity()
            {
                Map.KeyProperty.Generation = PrimaryKeyGeneration.Server;
            }

            public new IntegerKeyMappingDsl Column(string name)
            {
                base.Column(name);
                return this;
            }
        }

        public class GuidKeyMappingDsl : KeyMappingDsl
        {
            public GuidKeyMappingDsl(PropertyInfo property, ClassMap<T> map) :
                base(property, PrimaryKeyType.Guid, map) { }

            public void GuidComb()
            {
                Map.KeyProperty.Generation = PrimaryKeyGeneration.Client;
            }

            public new GuidKeyMappingDsl Column(string name)
            {
                base.Column(name);
                return this;
            }
        }
    }
}
