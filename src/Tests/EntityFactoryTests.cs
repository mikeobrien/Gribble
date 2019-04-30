using System.Collections.Generic;
using Gribble;
using Gribble.Mapping;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class EntityFactoryTests
    {
        public class EntityWithDefaultCtor
        {
            public string Static { get; set; }
        }

        public class EntityWithDefaultCtorMap : ClassMap<EntityWithDefaultCtor>
        {
            public EntityWithDefaultCtorMap()
            {
                Map(x => x.Static).Column("static_field");
            }
        }

        [Test]
        public void Should_create_entity_with_default_ctor()
        {
            var map = new EntityMapping(new EntityWithDefaultCtorMap());
            var values = new Dictionary<string, object>
            {
                { "static_field", "static value" }
            };
            var entity = new EntityFactory<EntityWithDefaultCtor>()
                .Create(values, map);

            entity.ShouldNotBeNull();
            entity.Static.ShouldEqual("static value");
        }

        public class EntityWithDefaultCtorAndDynamicProperty
        {
            public string Static { get; set; }
            public IDictionary<string, object> Dynamic { get; set; }
        }

        public class EntityWithDefaultCtorAndDynamicPropertyMap : 
            ClassMap<EntityWithDefaultCtorAndDynamicProperty>
        {
            public EntityWithDefaultCtorAndDynamicPropertyMap()
            {
                Map(x => x.Static).Column("static_field");
                Map(x => x.Dynamic).Dynamic();
            }
        }

        [Test]
        public void Should_create_entity_with_default_ctor_and_dynamic_property()
        {
            var map = new EntityMapping(new EntityWithDefaultCtorAndDynamicPropertyMap());
            var values = new Dictionary<string, object>
            {
                { "static_field", "static value" },
                { "dynamic_field_1", "dynamic value 1" },
                { "dynamic_field_2", "dynamic value 2" }
            };
            var entity = new EntityFactory<EntityWithDefaultCtorAndDynamicProperty>()
                .Create(values, map);

            entity.ShouldNotBeNull();
            entity.Static.ShouldEqual("static value");
            entity.Dynamic.Count.ShouldEqual(3);
            entity.Dynamic["Static"].ShouldEqual("static value");
            entity.Dynamic["dynamic_field_1"].ShouldEqual("dynamic value 1");
            entity.Dynamic["dynamic_field_2"].ShouldEqual("dynamic value 2");
        }

        public class AnonObject
        {
            public AnonObject(string Implicit)
            {
                this.Implicit = Implicit;
            }

            public string Implicit { get; set; }
        }

        [Test]
        public void Should_create_anon_object()
        {
            var map = new EntityMapping(new AutoClassMap<AnonObject>());
            var values = new Dictionary<string, object>
            {
                { "Implicit", "implicit value" }
            };
            var entity = new EntityFactory<AnonObject>()
                .Create(values, map);

            entity.ShouldNotBeNull();
            entity.Implicit.ShouldEqual("implicit value");
        }

        public class AnonObjectWithDynamicProperty
        {
            public AnonObjectWithDynamicProperty(string Implicit, IDictionary<string, object> Dynamic)
            {
                this.Implicit = Implicit;
                this.Dynamic = Dynamic;
            }

            public string Implicit { get; set; }
            public IDictionary<string, object> Dynamic { get; set; }
        }

        [Test]
        public void Should_create_anon_object_with_dynamic_property()
        {
            var map = new EntityMapping(new AutoClassMap<AnonObjectWithDynamicProperty>());
            var values = new Dictionary<string, object>
            {
                { "Implicit", "implicit value" },
                { "dynamic_field_1", "dynamic value 1" },
                { "dynamic_field_2", "dynamic value 2" }
            };
            var entity = new EntityFactory<AnonObjectWithDynamicProperty>()
                .Create(values, map);

            entity.ShouldNotBeNull();
            entity.Implicit.ShouldEqual("implicit value");
            entity.Dynamic.Count.ShouldEqual(3);
            entity.Dynamic["Implicit"].ShouldEqual("implicit value");
            entity.Dynamic["dynamic_field_1"].ShouldEqual("dynamic value 1");
            entity.Dynamic["dynamic_field_2"].ShouldEqual("dynamic value 2");
        }
    }
}
