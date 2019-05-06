using System;
using System.Collections.Generic;
using Gribble;
using Gribble.Mapping;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class EntityAdapterTests
    {
        public class Entity
        {
            public enum SomeState
            {
                Unknown,
                On,
                Off
            }

            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime Birthdate { get; set; }
            public DateTime? Created { get; set; }
            public int Age { get; set; }
            public float Price { get; set; }
            public double Distance { get; set; }
            public byte Flag { get; set; }
            public bool Active { get; set; }
            public decimal Length { get; set; }
            public long Miles { get; set; }
            public SomeState State { get; set; }
            public SomeState? NullableState { get; set; }
            public SomeState? NullableState2 { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        public class NoIdEntity
        {
            public string Name { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        private static readonly Guid Key = Guid.NewGuid();

        public class EntityMap : ClassMap<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id).Column("col_id");
                Map(x => x.Name).Column("col_name");
                Map(x => x.Birthdate).Column("col_birthdate");
                Map(x => x.Age).Column("col_age");
                Map(x => x.Price).Column("col_price");
                Map(x => x.Distance).Column("col_distance");
                Map(x => x.Flag).Column("col_flag");
                Map(x => x.Active).Column("col_active");
                Map(x => x.Length).Column("col_length");
                Map(x => x.Miles).Column("col_miles");
                Map(x => x.Created).Column("col_created");
                Map(x => x.State).Column("col_state");
                Map(x => x.NullableState).Column("col_state_null");
                Map(x => x.NullableState2).Column("col_state_null2");
                Map(x => x.Values).Dynamic();
            }
        }

        public class NoIdEntityMap : ClassMap<NoIdEntity>
        {
            public NoIdEntityMap()
            {
                Map(x => x.Name).Column("col_name");
                Map(x => x.Values).Dynamic();
            }
        }

        private static readonly EntityMapping Map = 
            new EntityMapping(new EntityMap(), new[] {
                new DynamicMapping("col_companyname", "CompanyName"),
                new DynamicMapping("col_optout", "OptOut"),
                new DynamicMapping("col_optoutdate", "OptOutDate")
            });

        private static readonly EntityMapping NoIdMap = new EntityMapping(new NoIdEntityMap(), new[] {
                new DynamicMapping("col_companyname", "CompanyName"),
                new DynamicMapping("col_optout", "OptOut"),
                new DynamicMapping("col_optoutdate", "OptOutDate")
            });

        private readonly Func<Entity> _createEntity = 
            () => new Entity {
                    Active = false,
                    Age = 44,
                    Birthdate = DateTime.MaxValue,
                    Created = DateTime.MinValue,
                    Distance = 22.3,
                    Flag = 23,
                    Id = Key,
                    Length = 22.3m,
                    Miles = 10,
                    Name = "oh hai",
                    Price = 45.67F,
                    Values = new Dictionary<string,object>
                            {
                                {"CompanyName", "Some company"},
                                {"OptOut", true},
                                {"OptOutDate", DateTime.MinValue},
                                {"col_optoutstatus", 1}
                            }
            };

        private readonly Func<NoIdEntity> _createNoIdEntity =
            () => new NoIdEntity {
                Name = "oh hai",
                Values = new Dictionary<string, object>
                            {
                                {"CompanyName", "Some company"},
                                {"OptOut", true},
                                {"OptOutDate", DateTime.MinValue},
                                {"col_optoutstatus", 1}
                            }
            };

        [Test]
        public void should_get_entity_fields()
        {
            var entity = _createEntity();
            var reader = new EntityAdapter<Entity>(entity, Map);
            var values = reader.GetValues();
            values.Count.ShouldEqual(18);

            values["col_active"].ShouldEqual(entity.Active);
            values["col_age"].ShouldEqual(entity.Age);
            values["col_birthdate"].ShouldEqual(entity.Birthdate);
            values["col_created"].ShouldEqual(entity.Created);
            values["col_distance"].ShouldEqual(entity.Distance);
            values["col_flag"].ShouldEqual(entity.Flag);
            values["col_id"].ShouldEqual(entity.Id);
            values["col_length"].ShouldEqual(entity.Length);
            values["col_miles"].ShouldEqual(entity.Miles);
            values["col_name"].ShouldEqual(entity.Name);
            values["col_price"].ShouldEqual(entity.Price);
            values["col_state"].ShouldEqual(entity.State);
            values["col_state_null"].ShouldEqual(entity.NullableState);
            values["col_state_null2"].ShouldEqual(entity.NullableState2);
            values["col_companyname"].ShouldEqual(entity.Values["CompanyName"]);
            values["col_optout"].ShouldEqual(entity.Values["OptOut"]);
            values["col_optoutdate"].ShouldEqual(entity.Values["OptOutDate"]);

            values["col_optoutstatus"].ShouldEqual(entity.Values["col_optoutstatus"]);
        }

        [Test]
        public void should_get_entity_with_no_id_fields()
        {
            var entity = _createNoIdEntity();
            var reader = new EntityAdapter<NoIdEntity>(entity, NoIdMap);
            var values = reader.GetValues();
            values.Count.ShouldEqual(5);

            values["col_name"].ShouldEqual(entity.Name);
            values["col_companyname"].ShouldEqual(entity.Values["CompanyName"]);
            values["col_optout"].ShouldEqual(entity.Values["OptOut"]);
            values["col_optoutdate"].ShouldEqual(entity.Values["OptOutDate"]);

            values["col_optoutstatus"].ShouldEqual(entity.Values["col_optoutstatus"]);
        }

        [Test]
        public void should_get_entity_key()
        {
            var entity = _createEntity();
            var reader = new EntityAdapter<Entity>(entity, Map);
            var key = reader.Key;
            key.ShouldEqual(Key);
        }

        [Test]
        public void should_set_entity_key()
        {
            var entity = new Entity();
            var reader = new EntityAdapter<Entity>(entity, Map);
            reader.Key = Key;
            entity.Id.ShouldEqual(Key);
        }
    }
}