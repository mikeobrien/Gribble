using System;
using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;
using Gribble.Statements;
using Gribble.TransactSql;
using NUnit.Framework;
using Should;

namespace Tests.TransactSql
{
    [TestFixture]
    public class UpdateWriterTests
    {
        public class Entity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime? Created { get; set; }
            public Dictionary<string, object> Values {get; set;}
        }

        public class EntityMap : ClassMap<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Created).Column("created");
                Map(x => x.Values).Dynamic();
            }
        }

        private static readonly EntityMapping Map = new EntityMapping(new EntityMap());
        private static readonly Guid Key = Guid.NewGuid();

        public const string TableName = "some_table_in_the_db";

        [Test]
        public void Update_Entity_Test()
        {
            var assignment = new Dictionary<string, object> { { "name", "bob" }, { "created", DateTime.MaxValue }, { "companyname", "Dunder Miflin" }, { "optout", true } };
            var update = new Update(assignment, TableName, Operator.Create.FieldEqualsConstant("Id", Key));
            var statement = UpdateWriter<Entity>.CreateStatement(update, Map);

            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(5);
            statement.Parameters.First().Value.ShouldEqual("bob");
            statement.Parameters.Skip(1).First().Value.ShouldEqual(DateTime.MaxValue);
            statement.Parameters.Skip(2).First().Value.ShouldEqual("Dunder Miflin");
            statement.Parameters.Skip(3).First().Value.ShouldEqual(true);
            statement.Parameters.Skip(4).First().Value.ShouldEqual(Key);
            statement.Text.ShouldEqual(
                string.Format("UPDATE [{0}] SET [name] = @{1}, [created] = @{2}, [companyname] = @{3}, [optout] = @{4} WHERE ([id] = @{5})", TableName,
                            statement.Parameters.First().Key,
                            statement.Parameters.Skip(1).First().Key,
                            statement.Parameters.Skip(2).First().Key,
                            statement.Parameters.Skip(3).First().Key,
                            statement.Parameters.Skip(4).First().Key));
        }
    }
}
