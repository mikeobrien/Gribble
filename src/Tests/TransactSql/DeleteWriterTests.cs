using System;
using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;
using Gribble.Model;
using Gribble.TransactSql;
using NUnit.Framework;
using Should;

namespace Tests.TransactSql
{
    [TestFixture]
    public class DeleteWriterTests
    {
        public class Entity
        {
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
            public IDictionary<string, object> Values {get; set;}
        }

        public class EntityMap : ClassMap<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Birthdate).Column("birthdate");
                Map(x => x.Created).Column("created");
                Map(x => x.Age).Column("age");
                Map(x => x.Price).Column("price");
                Map(x => x.Distance).Column("distance");
                Map(x => x.Flag).Column("flag");
                Map(x => x.Active).Column("active");
                Map(x => x.Length).Column("length");
                Map(x => x.Miles).Column("miles");
                Map(x => x.Values).Dynamic();
            }
        }

        private static readonly EntityMapping Map = new EntityMapping(new EntityMap());

        public const string TableName = "some_table_in_the_db";

        [Test]
        public void Delete_Single_Test()
        {
            var delete = new Delete(TableName, Operator.Create.FieldEqualsConstant("Id", Guid.Empty), false);
            var statement = DeleteWriter<Entity>.CreateStatement(delete, Map);
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(Guid.Empty);
            statement.Text.ShouldEqual(string.Format("DELETE TOP (1) FROM [{0}] WHERE ([id] = @{1})", TableName,
                            statement.Parameters.First().Key));
        }

        [Test]
        public void Delete_Multi_Test()
        {
            var delete = new Delete(TableName, Operator.Create.FieldAndConstant("Age", Operator.OperatorType.GreaterThan, 20), true);
            var statement = DeleteWriter<Entity>.CreateStatement(delete, Map);
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(20);
            statement.Text.ShouldEqual(string.Format("DELETE FROM [{0}] WHERE ([age] > @{1})", TableName,
                            statement.Parameters.First().Key));
        }

        [Test]
        public void should_render_multi_delete_by_query_sql()
        {
            var delete = new Delete(TableName, new Select { 
                From = { Type = Data.DataType.Table, Table = new Table { Name = TableName }}, 
                Where = Operator.Create.FieldAndConstant("Age", Operator.OperatorType.GreaterThan, 20)}, true);
            var statement = DeleteWriter<Entity>.CreateStatement(delete, Map);
            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(20);
            statement.Text.ShouldEqual(string.Format("DELETE FROM [{0}] WHERE EXISTS (SELECT [id] FROM (SELECT * FROM [{0}] {1} WHERE ([age] > @{2})) AS [__SubQuery__] WHERE [__SubQuery__].[id] = [{0}].[id])", 
                TableName, delete.Select.From.Alias, statement.Parameters.First().Key));
        }
    }
}
