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
    public class InsertWriterTests
    {
        public class IdentityEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime? Created { get; set; }
            public Dictionary<string, object> Values {get; set;}
        }

        public class GuidEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime? Created { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }

        public class IdentityEntityMap : ClassMap<IdentityEntity>
        {
            public IdentityEntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Created).Column("created");
                Map(x => x.Values).Dynamic();
            }
        }

        public class GuidEntityMap : ClassMap<GuidEntity>
        {
            public GuidEntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Created).Column("created");
                Map(x => x.Values).Dynamic();
            }
        }

        private static readonly EntityMapping IdentityMap = new EntityMapping(new IdentityEntityMap(), new[] { new ColumnMapping("companyname", "CompanyName"), new ColumnMapping("optout", "OptOut") });
        private static readonly EntityMapping GuidMap = new EntityMapping(new GuidEntityMap());

        public const string TableName1 = "XLIST_1";
        public const string TableName2 = "XLIST_2";

        [Test]
        public void Insert_Guid_Entity_Test()
        {
            var assignment = new Dictionary<string, object> {{"id", Guid.NewGuid()}, {"name", "bob"}, {"created", DateTime.MaxValue}, {"companyname", "Dunder Miflin"}, {"optout", true}};
            var insert = new Insert(assignment, false, TableName1);
            var statement = InsertWriter<GuidEntity>.CreateStatement(insert, GuidMap);

            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(5);
            statement.Parameters.First().Value.ShouldNotEqual(Guid.Empty);
            statement.Parameters.Skip(1).First().Value.ShouldEqual("bob");
            statement.Parameters.Skip(2).First().Value.ShouldEqual(DateTime.MaxValue);
            statement.Parameters.Skip(3).First().Value.ShouldEqual("Dunder Miflin");
            statement.Parameters.Skip(4).First().Value.ShouldEqual(true);
            statement.Text.ShouldEqual(
                string.Format("INSERT INTO [{0}] ([id], [name], [created], [companyname], [optout]) VALUES (@{1}, @{2}, @{3}, @{4}, @{5})", TableName1,
                            statement.Parameters.First().Key,
                            statement.Parameters.Skip(1).First().Key,
                            statement.Parameters.Skip(2).First().Key,
                            statement.Parameters.Skip(3).First().Key,
                            statement.Parameters.Skip(4).First().Key));
        }

        [Test]
        public void Insert_Identity_Entity_Test()
        {
            var assignment = new Dictionary<string, object> { { "name", "bob" }, { "created", DateTime.MaxValue }, { "companyname", "Dunder Miflin" }, { "optout", true } };
            var insert = new Insert(assignment, true, TableName1);
            var statement = InsertWriter<IdentityEntity>.CreateStatement(insert, IdentityMap);

            statement.Result.ShouldEqual(Statement.ResultType.Scalar);
            statement.Parameters.Count.ShouldEqual(4);
            statement.Parameters.First().Value.ShouldEqual("bob");
            statement.Parameters.Skip(1).First().Value.ShouldEqual(DateTime.MaxValue);
            statement.Parameters.Skip(2).First().Value.ShouldEqual("Dunder Miflin");
            statement.Parameters.Skip(3).First().Value.ShouldEqual(true);
            statement.Text.ShouldEqual(
                string.Format("INSERT INTO [{0}] ([name], [created], [companyname], [optout]) VALUES (@{1}, @{2}, @{3}, @{4}); SELECT CAST(SCOPE_IDENTITY() AS int)", TableName1,
                            statement.Parameters.First().Key,
                            statement.Parameters.Skip(1).First().Key,
                            statement.Parameters.Skip(2).First().Key,
                            statement.Parameters.Skip(3).First().Key));
        }

        [Test]
        public void Insert_Into_Test()
        {
            var fields = new List<string> { "name", "created", "companyname", "optout" };
            var properties = new List<string> { "Name", "Created", "CompanyName", "OptOut" };
            var select = new Select { Projection = properties.Select(x => new SelectProjection { Projection = Projection.Create.Field(x, x == "CompanyName" || x == "OptOut")}).ToList(),
                                      Source = { Type = Data.DataType.Table,
                                                 Table = new Table { Name = TableName2}} };
            var insert = new Insert(select, fields, TableName1);
            var statement = InsertWriter<IdentityEntity>.CreateStatement(insert, IdentityMap);

            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("INSERT INTO [{0}] ([name], [created], [companyname], [optout]) SELECT [name], [created], [companyname], [optout] FROM [{1}] {2}", 
                                                    TableName1, TableName2, select.Source.Alias));
        }
    }
}
