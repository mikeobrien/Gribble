using System;
using System.Collections.Generic;
using Gribble.Mapping;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class AutoClassMapTests
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

        [Test]
        public void should_use_property_names_for_column_names_with_identity_key()
        {
            var map = new AutoClassMap<IdentityEntity>();
            map.Type.ShouldEqual(typeof(IdentityEntity));
            map.Properties.Count.ShouldEqual(3);
            map.Properties[0].Property.Name.ShouldEqual("Id");
            map.Properties[0].ColumnName.ShouldEqual("Id");
            map.Properties[1].Property.Name.ShouldEqual("Name");
            map.Properties[1].ColumnName.ShouldEqual("Name");
            map.Properties[2].Property.Name.ShouldEqual("Age");
            map.Properties[2].ColumnName.ShouldEqual("Age");
            map.DynamicProperty.ShouldNotBeNull();
            map.DynamicProperty.Name.ShouldEqual("Values");
            map.KeyProperty.Type.ShouldEqual(PrimaryKeyType.Integer);
            map.KeyProperty.Generation.ShouldEqual(PrimaryKeyGeneration.Server);
            map.KeyProperty.Property.Name.ShouldEqual("Id");
            map.KeyProperty.ColumnName.ShouldEqual("Id");
        }

        [Test]
        public void should_use_property_names_for_column_names_with_guid_key()
        {
            var map = new AutoClassMap<GuidEntity>();
            map.Type.ShouldEqual(typeof(GuidEntity));
            map.Properties.Count.ShouldEqual(3);
            map.Properties[0].Property.Name.ShouldEqual("Id");
            map.Properties[0].ColumnName.ShouldEqual("Id");
            map.Properties[1].Property.Name.ShouldEqual("Name");
            map.Properties[1].ColumnName.ShouldEqual("Name");
            map.Properties[2].Property.Name.ShouldEqual("Age");
            map.Properties[2].ColumnName.ShouldEqual("Age");
            map.DynamicProperty.ShouldNotBeNull();
            map.DynamicProperty.Name.ShouldEqual("Values");
            map.KeyProperty.Type.ShouldEqual(PrimaryKeyType.Guid);
            map.KeyProperty.Generation.ShouldEqual(PrimaryKeyGeneration.Client);
            map.KeyProperty.Property.Name.ShouldEqual("Id");
            map.KeyProperty.ColumnName.ShouldEqual("Id");
        }
    }
}
