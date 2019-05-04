using System;
using System.Collections.Generic;
using Gribble.Mapping;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class EntityMappingTests
    {
        public class IdentityEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IDictionary<string, object> Values {get; set;}
        }

        public class GuidEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        public class NonDynamicEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class IdentityEntityMap : ClassMap<IdentityEntity>
        {
            public IdentityEntityMap()
            {
                Id(x => x.Id).Column("entity_id").Identity();
                Map(x => x.Name).Column("entity_name");
                Map(x => x.Values).Dynamic();
            }
        }

        public class GuidEntityMap : ClassMap<GuidEntity>
        {
            public GuidEntityMap()
            {
                Id(x => x.Id).Column("entity_id").GuidComb();
                Map(x => x.Name).Column("entity_name");
                Map(x => x.Values).Dynamic();
            }
        }

        public class NonDynamicEntityMap : ClassMap<GuidEntity>
        {
            public NonDynamicEntityMap()
            {
                Id(x => x.Id).Column("entity_id").GuidComb();
                Map(x => x.Name).Column("entity_name");
            }
        }

        [Test]
        public void Is_Identity_Key_Test()
        {
            var mapping = new EntityMapping(new IdentityEntityMap());
            mapping.KeyType.ShouldEqual(PrimaryKeyType.Integer);
            mapping.KeyGeneration.ShouldEqual(PrimaryKeyGeneration.Server);
        }

        [Test]
        public void Is_Not_Identity_Key_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.KeyType.ShouldEqual(PrimaryKeyType.Guid);
            mapping.KeyGeneration.ShouldEqual(PrimaryKeyGeneration.Client);
        }

        [Test]
        public void Key_Column_Name_Test()
        {
            var mapping = new EntityMapping(new IdentityEntityMap());
            mapping.Key.ColumnName.ShouldEqual("entity_id");
        }

        [Test]
        public void Key_Property_Name_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Key.Property.Name.ShouldEqual("Id");
        }

        [Test]
        public void Property_Has_Id_Column_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.StaticProperty.HasColumnMapping("Id").ShouldEqual(true);
        }

        [Test]
        public void Property_Has_Non_Id_Column_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.StaticProperty.HasColumnMapping("Name").ShouldEqual(true);
        }

        [Test]
        public void Property_Does_Not_Has_Column_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.StaticProperty.HasColumnMapping("SomeNonExistantColumn").ShouldEqual(false);
        }

        [Test]
        public void Property_Id_Column_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.StaticProperty.GetColumnName("Id").ShouldEqual("entity_id");
        }

        [Test]
        public void Property_Non_Id_Column_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.StaticProperty.GetColumnName("Name").ShouldEqual("entity_name");
        }

        [Test]
        public void Dynamic_Property_Default_Mapping_Has_Column_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.DynamicProperty.HasColumnMapping("SomeProperty").ShouldEqual(true);
        }

        [Test]
        public void Dynamic_Property_Default_Mapping_Column_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.DynamicProperty.GetColumnName("SomeProperty").ShouldEqual("SomeProperty");
        }

        [Test]
        public void Dynamic_Property_Has_Custom_Column_Mapping_Test()
        {
            var columnMapping = new[] { new DynamicMapping("some_property", "SomeProperty"), new DynamicMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.HasColumnMapping("SomeProperty").ShouldEqual(true);
        }

        [Test]
        public void Dynamic_Property_Does_Not_Have_Custom_Column_Mapping_Test()
        {
            var columnMapping = new[] { new DynamicMapping("some_property", "SomeProperty"), new DynamicMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.HasColumnMapping("SomeProperty2").ShouldEqual(false);
        }

        [Test]
        public void Dynamic_Property_Custom_Column_Mapping_Test()
        {
            var columnMapping = new[] { new DynamicMapping("some_property", "SomeProperty"), new DynamicMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.GetColumnName("AnotherProperty").ShouldEqual("another_property");
        }

        [Test]
        public void Has_Dynamic_Property_Test()
        {
            var columnMapping = new[] { new DynamicMapping("some_property", "SomeProperty"), new DynamicMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.HasProperty.ShouldEqual(true);
        }

        [Test]
        public void Does_Not_Have_Dynamic_Property_Test()
        {
            var mapping = new EntityMapping(new NonDynamicEntityMap());
            mapping.DynamicProperty.HasProperty.ShouldEqual(false);
        }

        [Test]
        public void Dynamic_Property_Name_Test_Test()
        {
            var columnMapping = new[] { new DynamicMapping("some_property", "SomeProperty"), new DynamicMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.Property.Name.ShouldEqual("Values");
        }

        [Test]
        public void Column_Has_Id_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.HasStaticPropertyMapping("entity_id").ShouldEqual(true);
        }

        [Test]
        public void Column_Has_Non_Id_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.HasStaticPropertyMapping("entity_name").ShouldEqual(true);
        }

        [Test]
        public void Column_Does_Not_Has_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.HasStaticPropertyMapping("entity_doesent_exist").ShouldEqual(false);
        }

        [Test]
        public void Mapping_Id_Column_Property_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetStaticProperty("entity_id").Name.ShouldEqual("Id");
        }

        [Test]
        public void should_case_insensitively_map_id_property_to_column_name()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetStaticProperty("Entity_Id").Name.ShouldEqual("Id");
        }

        [Test]
        public void Column_Non_Id_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetStaticProperty("entity_name").Name.ShouldEqual("Name");
        }

        [Test]
        public void should_case_insensitively_map_static_property_to_column_name()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetStaticProperty("Entity_Name").Name.ShouldEqual("Name");
        }

        [Test]
        public void Column_Default_Mapping_Has_Dynamic_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.HasDynamicMapping("some_column").ShouldEqual(true);
        }

        [Test]
        public void Column_Default_Mapping_Dynamic_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetDynamicName("some_column").ShouldEqual("some_column");
        }

        [Test]
        public void Column_Has_Custom_Dynamic_Property_Mapping_Test()
        {
            var columnMapping = new[] { new DynamicMapping("some_property", "SomeProperty"), new DynamicMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.Column.HasDynamicMapping("some_property").ShouldEqual(true);
        }

        [Test]
        public void Column_Does_Not_Have_Custom_Dynamic_Property_Mapping_Test()
        {
            var columnMapping = new[] { new DynamicMapping("some_property", "SomeProperty"), new DynamicMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.Column.HasDynamicMapping("some_property2").ShouldEqual(false);
        }

        [Test]
        public void Column_Custom_Dynamic_Property_Mapping_Test()
        {
            var columnMapping = new[] { new DynamicMapping("some_property", "SomeProperty"), new DynamicMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.Column.GetDynamicName("another_property").ShouldEqual("AnotherProperty");
        }
    }
}
