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
        public DynamicMap DynamicProperty { get; set; }
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

        public DynamicDsl Map(Expression<Func<T, IDictionary<string, object>>> property)
        {
            return new DynamicDsl(property.GetProperty(), this);
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
            new DynamicDsl(property, this).Dynamic();
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
            protected readonly ClassMap<T> ClassMap;
            protected readonly PropertyInfo Property;

            protected MappingDslBase(PropertyInfo property, ClassMap<T> classMap)
            {
                ClassMap = classMap;
                Property = property;
            }
        }

        public class ColumnMappingDsl : MappingDslBase
        {
            public ColumnMappingDsl(PropertyInfo property, ClassMap<T> classMap) : base(property, classMap)
            {
                Column(property.Name);
            }

            public MappingConfigDsl Column(string name)
            {
                return new MappingConfigDsl(ClassMap.AddMapping(name, Property));
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

        public class DynamicDsl : MappingDslBase
        {
            public DynamicDsl(PropertyInfo property, ClassMap<T> classMap) : base(property, classMap) { }

            public DynamicMappingDsl Dynamic()
            {
                return new DynamicMappingDsl(ClassMap.DynamicProperty = new DynamicMap(Property));
            }
        }

        public class DynamicMappingDsl
        {
            private readonly DynamicMap _map;

            public DynamicMappingDsl(DynamicMap map)
            {
                _map = map;
            }

            public DynamicColumnMappingDsl Map(string name)
            {
                return new DynamicColumnMappingDsl(_map, name);
            }
        }

        public class DynamicColumnMappingDsl
        {
            private readonly DynamicMap _map;
            private readonly string _name;

            public DynamicColumnMappingDsl(DynamicMap map, string name)
            {
                _map = map;
                _name = name;
            }

            public DynamicColumnMappingConfigDsl Column(string columnName)
            {
                var mapping = new DynamicMapping(columnName, _name);
                _map.Mapping.Add(mapping);
                return new DynamicColumnMappingConfigDsl(mapping);
            }
        }

        public class DynamicColumnMappingConfigDsl
        {
            private readonly DynamicMapping _mapping;

            public DynamicColumnMappingConfigDsl(DynamicMapping mapping)
            {
                _mapping = mapping;
            }

            public void Readonly()
            {
                _mapping.Readonly = true;
            }
        }

        public class KeyMappingDsl : MappingDslBase
        {
            private readonly PrimaryKeyType _type;

            public KeyMappingDsl(PropertyInfo property, PrimaryKeyType type, ClassMap<T> classMap) : 
                base(property, classMap)
            {
                _type = type;
            }

            public virtual KeyMappingDsl Column(string name)
            {
                ClassMap.AddKeyMapping(_type, name, Property);
                return this;
            }
        }

        public class IntegerKeyMappingDsl : KeyMappingDsl
        {
            public IntegerKeyMappingDsl(PropertyInfo property, ClassMap<T> classMap) :
                base(property, PrimaryKeyType.Integer, classMap) { }

            public void Identity()
            {
                ClassMap.KeyProperty.Generation = PrimaryKeyGeneration.Server;
            }

            public new IntegerKeyMappingDsl Column(string name)
            {
                base.Column(name);
                return this;
            }
        }

        public class GuidKeyMappingDsl : KeyMappingDsl
        {
            public GuidKeyMappingDsl(PropertyInfo property, ClassMap<T> classMap) :
                base(property, PrimaryKeyType.Guid, classMap) { }

            public void GuidComb()
            {
                ClassMap.KeyProperty.Generation = PrimaryKeyGeneration.Client;
            }

            public new GuidKeyMappingDsl Column(string name)
            {
                base.Column(name);
                return this;
            }
        }
    }
}
