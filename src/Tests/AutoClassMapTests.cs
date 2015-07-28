using System;
using System.Collections.Generic;
using System.Linq;
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
            public Dictionary<string, object> Values { get; set; }
        }

        public class GuidEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }

        [Test]
        public void should_use_property_names_for_column_names_with_identity_key()
        {
            var map = new AutoClassMap<IdentityEntity>();
            map.Type.ShouldEqual(typeof(IdentityEntity));
            map.PropertyColumMapping.Count.ShouldEqual(3);
            map.PropertyColumMapping.ElementAt(0).Key.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(0).Value.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(1).Key.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(1).Value.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(2).Key.ShouldEqual("Age");
            map.PropertyColumMapping.ElementAt(2).Value.ShouldEqual("Age");
            map.HasDynamicProperty.ShouldEqual(true);
            map.DynamicProperty.ShouldEqual("Values");
            map.KeyType.ShouldEqual(PrimaryKeyType.IdentitySeed);
            map.KeyProperty.ShouldEqual("Id");
            map.KeyColumn.ShouldEqual("Id");
        }

        [Test]
        public void should_use_property_names_for_column_names_with_guid_key()
        {
            var map = new AutoClassMap<GuidEntity>();
            map.Type.ShouldEqual(typeof(GuidEntity));
            map.PropertyColumMapping.Count.ShouldEqual(3);
            map.PropertyColumMapping.ElementAt(0).Key.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(0).Value.ShouldEqual("Id");
            map.PropertyColumMapping.ElementAt(1).Key.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(1).Value.ShouldEqual("Name");
            map.PropertyColumMapping.ElementAt(2).Key.ShouldEqual("Age");
            map.PropertyColumMapping.ElementAt(2).Value.ShouldEqual("Age");
            map.HasDynamicProperty.ShouldEqual(true);
            map.DynamicProperty.ShouldEqual("Values");
            map.KeyType.ShouldEqual(PrimaryKeyType.GuidClientGenerated);
            map.KeyProperty.ShouldEqual("Id");
            map.KeyColumn.ShouldEqual("Id");
        }
    }
}
