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

        private static readonly Dictionary<string, object> EntityValues = 
            new Dictionary<string, object>
            {
                {"col_active", false},
                {"col_age", 44},
                {"col_birthdate", DateTime.MaxValue},
                {"col_created", DateTime.MinValue},
                {"col_distance", 22.3},
                {"col_flag", (byte)23},
                {"col_id", Key},
                {"col_length", 22.3m},
                {"col_miles", 10L},
                {"col_name", "oh hai"},
                {"col_price", 45.67F},
                {"col_companyname", "Some company"},
                {"col_optout", true},
                {"col_optoutdate", DateTime.MinValue},
                {"col_state", 2},
                {"col_state_null", 1},
                {"col_state_null2", null}
            };

        private static readonly Dictionary<string, object> NoIdEntityValues =
            new Dictionary<string, object>
            {
                {"col_name", "oh hai"},
                {"col_companyname", "Some company"},
                {"col_optout", true},
                {"col_optoutdate", DateTime.MinValue},
            };

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
                new ColumnMapping("col_companyname", "CompanyName"),
                new ColumnMapping("col_optout", "OptOut"),
                new ColumnMapping("col_optoutdate", "OptOutDate")
            });

        private static readonly EntityMapping NoIdMap = new EntityMapping(new NoIdEntityMap(), new[] {
                new ColumnMapping("col_companyname", "CompanyName"),
                new ColumnMapping("col_optout", "OptOut"),
                new ColumnMapping("col_optoutdate", "OptOutDate")
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
                                {"OptOutDate", DateTime.MinValue}
                            }
            };

        private readonly Func<NoIdEntity> _createNoIdEntity =
            () => new NoIdEntity {
                Name = "oh hai",
                Values = new Dictionary<string, object>
                            {
                                {"CompanyName", "Some company"},
                                {"OptOut", true},
                                {"OptOutDate", DateTime.MinValue}
                            }
            };

        [Test]
        public void should_get_entity_fields()
        {
            var entity = _createEntity();
            var reader = new EntityAdapter<Entity>(entity, Map);
            var values = reader.GetValues();
            values.Count.ShouldEqual(EntityValues.Count);

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
        }

        [Test]
        public void should_get_entity_with_no_id_fields()
        {
            var entity = _createNoIdEntity();
            var reader = new EntityAdapter<NoIdEntity>(entity, NoIdMap);
            var values = reader.GetValues();
            values.Count.ShouldEqual(NoIdEntityValues.Count);

            values["col_name"].ShouldEqual(entity.Name);
            values["col_companyname"].ShouldEqual(entity.Values["CompanyName"]);
            values["col_optout"].ShouldEqual(entity.Values["OptOut"]);
            values["col_optoutdate"].ShouldEqual(entity.Values["OptOutDate"]);
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