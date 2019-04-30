using System;
using System.Collections.Generic;
using System.Linq;
using Gribble;
using Gribble.Mapping;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class EntityDictionaryTests
    {
        public class Entity
        {
            public string StaticProperty1 { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        public class EntityMap : ClassMap<Entity>
        {
            public EntityMap()
            {
                Map(x => x.StaticProperty1).Column("static_prop_1");
            }
        }

        [Test]
        public void Should_get_and_set_static_value()
        {
            var entity = new Entity
            {
                StaticProperty1 = "fark"
            };
            var dictionary = CreateDictionary(entity);

            dictionary["StaticProperty1"].ShouldEqual("fark");
            entity.StaticProperty1.ShouldEqual("fark");

            dictionary["StaticProperty1"] = "new sp1";

            dictionary["StaticProperty1"].ShouldEqual("new sp1");
            entity.StaticProperty1.ShouldEqual("new sp1");
        }

        [Test]
        public void Should_get_and_set_static_values_with_override()
        {
            var entity = new Entity
            {
                StaticProperty1 = "fark"
            };
            var dictionary = CreateDictionary(entity, new ColumnMapping("static_prop_1", "dsp1"));

            Assert.Throws<KeyNotFoundException>(() => dictionary["StaticProperty1"].ShouldEqual("fark"));

            dictionary["dsp1"].ShouldEqual("fark");
            entity.StaticProperty1.ShouldEqual("fark");

            dictionary["dsp1"] = "new sp1";

            dictionary["dsp1"].ShouldEqual("new sp1");
            entity.StaticProperty1.ShouldEqual("new sp1");
        }

        [Test]
        public void Should_get_and_set_dynamic_value()
        {
            var entity = new Entity();
            var dictionary = CreateDictionary(entity);

            dictionary["dynamic_1"].ShouldEqual("d1");

            dictionary["dynamic_1"] = "new d1";

            dictionary["dynamic_1"].ShouldEqual("new d1");
        }

        [Test]
        public void Should_try_get_static_value()
        {
            var entity = new Entity
            {
                StaticProperty1 = "fark"
            };
            var dictionary = CreateDictionary(entity);
            
            dictionary.TryGetValue("StaticProperty1", out var value).ShouldBeTrue();
                
            value.ShouldEqual("fark");
        }

        [Test]
        public void Should_try_get_static_values_with_override()
        {
            var entity = new Entity
            {
                StaticProperty1 = "fark"
            };
            var dictionary = CreateDictionary(entity, new ColumnMapping("static_prop_1", "dsp1"));
            
            dictionary.TryGetValue("StaticProperty1", out _).ShouldBeFalse();
            
            dictionary.TryGetValue("dsp1", out var value).ShouldBeTrue();
                
            value.ShouldEqual("fark");
        }

        [Test]
        public void Should_try_get_dynamic_value()
        {
            var dictionary = CreateDictionary(new Entity());

            dictionary.TryGetValue("dynamic_1", out var value).ShouldBeTrue();
                
            value.ShouldEqual("d1");
        }

        [Test]
        public void Should_get_item_count()
        {
            var entity = new Entity();
            var dictionary = CreateDictionary(entity);

            dictionary.Count.ShouldEqual(2);
        }

        [Test]
        public void Should_get_enumerator()
        {
            var entity = new Entity
            {
                StaticProperty1 = "sp1"
            };
            var dictionary = CreateDictionary(entity);

            var enumerator = dictionary.GetEnumerator();
            
            enumerator.MoveNext().ShouldBeTrue();

            enumerator.Current.Key.ShouldEqual("dynamic_1");
            enumerator.Current.Value.ShouldEqual("d1");

            enumerator.MoveNext().ShouldBeTrue();

            enumerator.Current.Key.ShouldEqual("StaticProperty1");
            enumerator.Current.Value.ShouldEqual("sp1");

            enumerator.MoveNext().ShouldBeFalse();
        }

        [Test]
        public void Should_get_enumerator_with_overriden_static_mapping()
        {
            var entity = new Entity
            {
                StaticProperty1 = "sp1"
            };
            var dictionary = CreateDictionary(entity, new ColumnMapping("static_prop_1", "dsp1"));

            var enumerator = dictionary.GetEnumerator();
            
            enumerator.MoveNext().ShouldBeTrue();

            enumerator.Current.Key.ShouldEqual("dynamic_1");
            enumerator.Current.Value.ShouldEqual("d1");

            enumerator.MoveNext().ShouldBeTrue();

            enumerator.Current.Key.ShouldEqual("dsp1");
            enumerator.Current.Value.ShouldEqual("sp1");

            enumerator.MoveNext().ShouldBeFalse();
        }

        [Test]
        public void Should_add_item()
        {
            var entity = new Entity
            {
                StaticProperty1 = "sp1"
            };
            var dictionary = CreateDictionary(entity);

            dictionary.Add("dynamic_2", "d2");

            Should_contain_added_item(dictionary);
        }

        [Test]
        public void Should_add_key_value_pair()
        {
            var entity = new Entity
            {
                StaticProperty1 = "sp1"
            };
            var dictionary = CreateDictionary(entity);

            dictionary.Add(new KeyValuePair<string, object>("dynamic_2", "d2"));

            Should_contain_added_item(dictionary);
        }

        private void Should_contain_added_item(IDictionary<string, object> dictionary)
        {
            dictionary.Count.ShouldEqual(3);

            var values = dictionary.ToList();

            values[0].Key.ShouldEqual("dynamic_1");
            values[0].Value.ShouldEqual("d1");

            values[1].Key.ShouldEqual("dynamic_2");
            values[1].Value.ShouldEqual("d2");

            values[2].Key.ShouldEqual("StaticProperty1");
            values[2].Value.ShouldEqual("sp1");
        }

        [Test]
        public void Should_contain_static_property()
        {
            var dictionary = CreateDictionary(new Entity());

            dictionary.ContainsKey("StaticProperty1").ShouldBeTrue();
        }

        [Test]
        public void Should_contain_key_value_pair()
        {
            var dictionary = CreateDictionary(new Entity());

            dictionary.Contains(new KeyValuePair<string, object>("StaticProperty1", null)).ShouldBeTrue();
        }

        [Test]
        public void Should_contain_overidden_static_property()
        {
            var dictionary = CreateDictionary(new Entity(), 
                new ColumnMapping("static_prop_1", "dsp1"));

            dictionary.ContainsKey("dsp1").ShouldBeTrue();
            dictionary.ContainsKey("StaticProperty1").ShouldBeFalse();
        }

        [Test]
        public void Should_contain_dynamic_value()
        {
            var dictionary = CreateDictionary(new Entity());

            dictionary.ContainsKey("dynamic_1").ShouldBeTrue();
        }

        [Test]
        public void Should_remove_dynamic_value()
        {
            var dictionary = CreateDictionary(new Entity());

            dictionary.ContainsKey("dynamic_1").ShouldBeTrue();
            dictionary.Count.ShouldEqual(2);

            dictionary.Remove("dynamic_1").ShouldBeTrue();

            dictionary.ContainsKey("dynamic_1").ShouldBeFalse();
            dictionary.Count.ShouldEqual(1);
        }

        [Test]
        public void Should_remove_dynamic_value_value_pair()
        {
            var dictionary = CreateDictionary(new Entity());

            dictionary.ContainsKey("dynamic_1").ShouldBeTrue();
            dictionary.Count.ShouldEqual(2);

            dictionary.Remove(new KeyValuePair<string, object>("dynamic_1", null)).ShouldBeTrue();

            dictionary.ContainsKey("dynamic_1").ShouldBeFalse();
            dictionary.Count.ShouldEqual(1);
        }

        [Test]
        public void Should_not_remove_static_property()
        {
            var dictionary = CreateDictionary(new Entity());

            dictionary.ContainsKey("StaticProperty1").ShouldBeTrue();
            dictionary.Count.ShouldEqual(2);

            dictionary.Remove("StaticProperty1").ShouldBeFalse();

            dictionary.ContainsKey("StaticProperty1").ShouldBeTrue();
            dictionary.Count.ShouldEqual(2);
        }

        [Test]
        public void Should_get_keys()
        {
            var dictionary = CreateDictionary(new Entity());

            var keys = dictionary.Keys.ToList();

            keys.Count.ShouldEqual(2);

            keys[0].ShouldEqual("dynamic_1");
            keys[1].ShouldEqual("StaticProperty1");
        }

        [Test]
        public void Should_get_values()
        {
            var dictionary = CreateDictionary(new Entity
            {
                StaticProperty1 = "sp1"
            });

            var values = dictionary.Values.ToList();

            values.Count.ShouldEqual(2);

            values[0].ShouldEqual("d1");
            values[1].ShouldEqual("sp1");
        }

        [Test]
        public void Should_copy_to_array()
        {
            var dictionary = CreateDictionary(new Entity
            {
                StaticProperty1 = "sp1"
            });

            var array = new KeyValuePair<string, object>[2];

            dictionary.CopyTo(array, 0);

            array[0].Key.ShouldEqual("dynamic_1");
            array[0].Value.ShouldEqual("d1");

            array[1].Key.ShouldEqual("StaticProperty1");
            array[1].Value.ShouldEqual("sp1");
        }

        private EntityDictionary CreateDictionary(Entity entity, params ColumnMapping[] mappingOverride)
        {
            var dictionary = new EntityDictionary(new EntityMapping(new EntityMap(), mappingOverride));
            dictionary.Init(entity, new Dictionary<string, object>
            {
                { "dynamic_1", "d1" }
            });
            return dictionary;
        }
    }
}
