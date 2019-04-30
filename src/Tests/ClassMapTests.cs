using System;
using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class ClassMapTests
    {
        public class IdentityEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        public class GuidEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        public class IdentityMap : ClassMap<IdentityEntity>
        {
            public IdentityMap()
            {
                Id(x => x.Id).Column("col_id").Identity();
                Map(x => x.Name).Column("col_name");
                Map(x => x.Age).Column("col_age");
                Map(x => x.Values).Dynamic();
            }
        }

        public class GuidServerMap : ClassMap<GuidEntity>
        {
            public GuidServerMap()
            {
                Id(x => x.Id).Column("col_id").GuidComb();
                Map(x => x.Name).Column("col_name");
                Map(x => x.Age).Column("col_age");
                Map(x => x.Values).Dynamic();
            }
        }

        public class GuidClientMap : ClassMap<GuidEntity>
        {
            public GuidClientMap()
            {
                Id(x => x.Id).Column("col_id").GuidComb();
                Map(x => x.Name).Column("col_name");
                Map(x => x.Age).Column("col_age");
                Map(x => x.Values).Dynamic();
            }
        }

        public class IdentityPropertyNameConventionMap : ClassMap<IdentityEntity>
        {
            public IdentityPropertyNameConventionMap()
            {
                Id(x => x.Id).Identity();
                Map(x => x.Name);
                Map(x => x.Age);
                Map(x => x.Values).Dynamic();
            }
        }

        public class GuidPropertyNameConventionMap : ClassMap<GuidEntity>
        {
            public GuidPropertyNameConventionMap()
            {
                Id(x => x.Id).GuidComb();
                Map(x => x.Name);
                Map(x => x.Age);
                Map(x => x.Values).Dynamic();
            }
        }

        [Test]
        public void should_use_property_names_for_column_names_with_identity_key()
        {
            var map = new IdentityPropertyNameConventionMap();
            map.Type.ShouldEqual(typeof(IdentityEntity));
            map.PropertyColumMapping.Count.ShouldEqual(3);
            map.PropertyColumMapping.ElementAt(0).Key.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(0).Value.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(1).Key.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(1).Value.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(2).Key.ShouldEqual("Age");
            map.PropertyColumMapping.ElementAt(2).Value.ShouldEqual("Age");
            map.HasDynamicProperty.ShouldEqual(true);
            map.DynamicProperty.Name.ShouldEqual("Values");
            map.KeyType.ShouldEqual(PrimaryKeyType.Integer);
            map.KeyGeneration.ShouldEqual(PrimaryKeyGeneration.Server);
            map.KeyProperty.Name.ShouldEqual("Id");
            map.KeyColumn.ShouldEqual("Id");
        }

        [Test]
        public void should_use_property_names_for_column_names_with_guid_key()
        {
            var map = new GuidPropertyNameConventionMap();
            map.Type.ShouldEqual(typeof(GuidEntity));
            map.PropertyColumMapping.Count.ShouldEqual(3);
            map.PropertyColumMapping.ElementAt(0).Key.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(0).Value.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(1).Key.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(1).Value.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(2).Key.ShouldEqual("Age");
            map.PropertyColumMapping.ElementAt(2).Value.ShouldEqual("Age");
            map.HasDynamicProperty.ShouldEqual(true);
            map.DynamicProperty.Name.ShouldEqual("Values");
            map.KeyType.ShouldEqual(PrimaryKeyType.Guid);
            map.KeyGeneration.ShouldEqual(PrimaryKeyGeneration.Client);
            map.KeyProperty.Name.ShouldEqual("Id");
            map.KeyColumn.ShouldEqual("Id");
        }

        [Test]
        public void Identity_Map_Test()
        {
            var map = new IdentityMap();
            map.Type.ShouldEqual(typeof(IdentityEntity));
            map.PropertyColumMapping.Count.ShouldEqual(3);
            map.PropertyColumMapping.ElementAt(0).Key.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(0).Value.ShouldEqual("col_id");
            map.PropertyColumMapping.ElementAt(1).Key.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(1).Value.ShouldEqual("col_name");
            map.PropertyColumMapping.ElementAt(2).Key.ShouldEqual("Age");
            map.PropertyColumMapping.ElementAt(2).Value.ShouldEqual("col_age");
            map.HasDynamicProperty.ShouldEqual(true);
            map.DynamicProperty.Name.ShouldEqual("Values");
            map.KeyType.ShouldEqual(PrimaryKeyType.Integer);
            map.KeyGeneration.ShouldEqual(PrimaryKeyGeneration.Server);
            map.KeyProperty.Name.ShouldEqual("Id");
            map.KeyColumn.ShouldEqual("col_id");
        }

        [Test]
        public void Guid_Map_Test()
        {
            var map = new GuidServerMap();
            map.Type.ShouldEqual(typeof(GuidEntity));
            map.PropertyColumMapping.Count.ShouldEqual(3);
            map.PropertyColumMapping.ElementAt(0).Key.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(0).Value.ShouldEqual("col_id");
            map.PropertyColumMapping.ElementAt(1).Key.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(1).Value.ShouldEqual("col_name");
            map.PropertyColumMapping.ElementAt(2).Key.ShouldEqual("Age");
            map.PropertyColumMapping.ElementAt(2).Value.ShouldEqual("col_age");
            map.HasDynamicProperty.ShouldEqual(true);
            map.DynamicProperty.Name.ShouldEqual("Values");
            map.KeyType.ShouldEqual(PrimaryKeyType.Guid);
            map.KeyGeneration.ShouldEqual(PrimaryKeyGeneration.Client);
            map.KeyProperty.Name.ShouldEqual("Id");
            map.KeyColumn.ShouldEqual("col_id");
        }

        [Test]
        public void Client_Guid_Map_Test()
        {
            var map = new GuidClientMap();
            map.Type.ShouldEqual(typeof(GuidEntity));
            map.PropertyColumMapping.Count.ShouldEqual(3);
            map.PropertyColumMapping.ElementAt(0).Key.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(0).Value.ShouldEqual("col_id");
            map.PropertyColumMapping.ElementAt(1).Key.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(1).Value.ShouldEqual("col_name");
            map.PropertyColumMapping.ElementAt(2).Key.ShouldEqual("Age");
            map.PropertyColumMapping.ElementAt(2).Value.ShouldEqual("col_age");
            map.HasDynamicProperty.ShouldEqual(true);
            map.DynamicProperty.Name.ShouldEqual("Values");
            map.KeyType.ShouldEqual(PrimaryKeyType.Guid);
            map.KeyGeneration.ShouldEqual(PrimaryKeyGeneration.Client);
            map.KeyProperty.Name.ShouldEqual("Id");
            map.KeyColumn.ShouldEqual("col_id");
        }
    }
}
