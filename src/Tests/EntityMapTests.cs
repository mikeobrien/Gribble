using System;
using System.Collections.Generic;
using Gribble.Mapping;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class EntityMapTests
    {
        public class IdentityEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> Values {get; set;}
        }

        public class GuidEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> Values { get; set; }
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
                Id(x => x.Id).Column("entity_id");
                Map(x => x.Name).Column("entity_name");
                Map(x => x.Values).Dynamic();
            }
        }

        public class GuidEntityMap : ClassMap<GuidEntity>
        {
            public GuidEntityMap()
            {
                Id(x => x.Id).Column("entity_id").Generated();
                Map(x => x.Name).Column("entity_name");
                Map(x => x.Values).Dynamic();
            }
        }

        public class NonDynamicEntityMap : ClassMap<GuidEntity>
        {
            public NonDynamicEntityMap()
            {
                Id(x => x.Id).Column("entity_id").Generated();
                Map(x => x.Name).Column("entity_name");
            }
        }

        [Test]
        public void Is_Identity_Key_Test()
        {
            var mapping = new EntityMapping(new IdentityEntityMap());
            mapping.Key.KeyType.ShouldEqual(PrimaryKeyType.IdentitySeed);
        }

        [Test]
        public void Is_Not_Identity_Key_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Key.KeyType.ShouldEqual(PrimaryKeyType.GuidClientGenerated);
        }

        [Test]
        public void Key_Column_Name_Test()
        {
            var mapping = new EntityMapping(new IdentityEntityMap());
            mapping.Key.GetColumnName().ShouldEqual("entity_id");
        }

        [Test]
        public void Key_Property_Name_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Key.GetPropertyName().ShouldEqual("Id");
        }

        [Test]
        public void Key_Generate_Id_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            var key = mapping.Key.GenerateGuidKey();

            key.ShouldNotEqual(Guid.Empty);
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
            var columnMapping = new[] { new ColumnMapping("some_property", "SomeProperty"), new ColumnMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.HasColumnMapping("SomeProperty").ShouldEqual(true);
        }

        [Test]
        public void Dynamic_Property_Does_Not_Have_Custom_Column_Mapping_Test()
        {
            var columnMapping = new[] { new ColumnMapping("some_property", "SomeProperty"), new ColumnMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.HasColumnMapping("SomeProperty2").ShouldEqual(false);
        }

        [Test]
        public void Dynamic_Property_Custom_Column_Mapping_Test()
        {
            var columnMapping = new[] { new ColumnMapping("some_property", "SomeProperty"), new ColumnMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.GetColumnName("AnotherProperty").ShouldEqual("another_property");
        }

        [Test]
        public void Has_Dynamic_Property_Test()
        {
            var columnMapping = new[] { new ColumnMapping("some_property", "SomeProperty"), new ColumnMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.HasProperty().ShouldEqual(true);
        }

        [Test]
        public void Does_Not_Have_Dynamic_Property_Test()
        {
            var mapping = new EntityMapping(new NonDynamicEntityMap());
            mapping.DynamicProperty.HasProperty().ShouldEqual(false);
        }

        [Test]
        public void Dynamic_Property_Name_Test_Test()
        {
            var columnMapping = new[] { new ColumnMapping("some_property", "SomeProperty"), new ColumnMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.DynamicProperty.GetPropertyName().ShouldEqual("Values");
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
            mapping.Column.GetStaticPropertyName("entity_id").ShouldEqual("Id");
        }

        [Test]
        public void should_case_insensitively_map_id_property_to_column_name()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetStaticPropertyName("Entity_Id").ShouldEqual("Id");
        }

        [Test]
        public void Column_Non_Id_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetStaticPropertyName("entity_name").ShouldEqual("Name");
        }

        [Test]
        public void should_case_insensitively_map_static_property_to_column_name()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetStaticPropertyName("Entity_Name").ShouldEqual("Name");
        }

        [Test]
        public void Column_Default_Mapping_Has_Dynamic_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.HasDynamicPropertyMapping("some_column").ShouldEqual(true);
        }

        [Test]
        public void Column_Default_Mapping_Dynamic_Property_Mapping_Test()
        {
            var mapping = new EntityMapping(new GuidEntityMap());
            mapping.Column.GetDynamicPropertyName("some_column").ShouldEqual("some_column");
        }

        [Test]
        public void Column_Has_Custom_Dynamic_Property_Mapping_Test()
        {
            var columnMapping = new[] { new ColumnMapping("some_property", "SomeProperty"), new ColumnMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.Column.HasDynamicPropertyMapping("some_property").ShouldEqual(true);
        }

        [Test]
        public void Column_Does_Not_Have_Custom_Dynamic_Property_Mapping_Test()
        {
            var columnMapping = new[] { new ColumnMapping("some_property", "SomeProperty"), new ColumnMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.Column.HasDynamicPropertyMapping("some_property2").ShouldEqual(false);
        }

        [Test]
        public void Column_Custom_Dynamic_Property_Mapping_Test()
        {
            var columnMapping = new[] { new ColumnMapping("some_property", "SomeProperty"), new ColumnMapping("another_property", "AnotherProperty") };
            var mapping = new EntityMapping(new GuidEntityMap(), columnMapping);
            mapping.Column.GetDynamicPropertyName("another_property").ShouldEqual("AnotherProperty");
        }
    }
}
