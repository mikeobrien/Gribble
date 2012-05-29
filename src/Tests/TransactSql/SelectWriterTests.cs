using System;
using System.Collections.Generic;
using System.Linq;
using Gribble;
using Gribble.Expressions;
using Gribble.Mapping;
using Gribble.Model;
using Gribble.TransactSql;
using NUnit.Framework;
using Should;

namespace Tests.TransactSql
{
    [TestFixture]
    public class SelectWriterTests
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
            public Dictionary<string, object> Values {get; set;}
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

        private static readonly EntityMapping Map = new EntityMapping(new EntityMap(), new [] {new ColumnMapping("companyname", "CompanyName")});

        private const string TableName1 = "XLIST_1";
        private const string TableName2 = "XLIST_2";
        private const string TableName3 = "XLIST_3";


        [Test]
        public void Select_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [{0}] {1}", TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Top_Count_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Take(5);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT TOP (5) * FROM [{0}] {1}", TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Top_Percent_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.TakePercent(5);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT TOP (5) PERCENT * FROM [{0}] {1}", TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Randomize_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Randomize();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [{0}] {1} ORDER BY NEWID()", TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_distinct_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Distinct(x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [age] ORDER BY [age] ASC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] = 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_multi_distinct_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Distinct(x => x.Name.ToUpper()).Distinct(x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY UPPER([name]), [age] ORDER BY UPPER([name]) ASC, [age] ASC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] = 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_distinct_with_order_ascending_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Distinct(x => x.Age, x => x.Name, Order.Ascending);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [age] ORDER BY [name] ASC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] = 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_distinct_with_order_descending_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Distinct(x => x.Age, x => x.Name, Order.Descending);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [age] ORDER BY [name] DESC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] = 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_duplicates_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Duplicates(x => x.Name);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [name] ORDER BY [name] ASC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] > 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_duplicates_with_precedence_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Duplicates(x => x.Name, x => x.Age > 50, Order.Ascending);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(50);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [name] ORDER BY CASE WHEN ([age] > @{2}) THEN 1 ELSE 0 END ASC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] > 1",
                                                    TableName1, select.From.Alias, statement.Parameters.First().Key));
        }

        [Test]
        public void should_render_duplicates_with_ascending_order_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Duplicates(x => x.Name, x => x.Age, Order.Ascending);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [name] ORDER BY [age] ASC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] > 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_duplicates_with_descending_order_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Duplicates(x => x.Name, x => x.Age, Order.Descending);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [name] ORDER BY [age] DESC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] > 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_duplicates_with_double_order_ascending_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Distinct(x => x.Name.ToUpper()).Duplicates(x => x.Name, x => x.Age, Order.Ascending, x => x.Created, Order.Descending);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [name] ORDER BY [age] ASC, [created] DESC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] > 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_duplicates_with_double_order_descending_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Duplicates(x => x.Name, x => x.Age, Order.Descending, x => x.Created, Order.Ascending);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY [name] ORDER BY [age] DESC, [created] ASC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] > 1",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Distinct_Skip_Take_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Distinct(x => x.Name.ToUpper()).Distinct(x => x.Age).Skip(10).Take(10);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY [id]) AS [__RowNumber__] FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY UPPER([name]), [age] ORDER BY UPPER([name]) ASC, [age] ASC) AS [__Partition__] FROM [{0}] {1}) AS {1} WHERE [__Partition__] = 1) AS {1} WHERE [__RowNumber__] BETWEEN 11 AND 20", 
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void Select_First_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.First();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Single);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT TOP (1) * FROM [{0}] {1}", TableName1, select.From.Alias));
        }

        [Test]
        public void Select_First_Or_Default_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.FirstOrDefault();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.SingleOrNone);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT TOP (1) * FROM [{0}] {1}", TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_any_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Any();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Scalar);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT CAST(CASE WHEN EXISTS (SELECT * FROM [{0}] {1}) THEN 1 ELSE 0 END AS bit)", TableName1, select.From.Alias));
        }

        [Test]
        public void should_render_any_and_where_sql()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Any(x => x.Age == 44);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Scalar);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(44);
            statement.Text.ShouldEqual(string.Format("SELECT CAST(CASE WHEN EXISTS (SELECT * FROM [{0}] {1} WHERE ([age] = @{2})) THEN 1 ELSE 0 END AS bit)",
                TableName1, select.From.Alias, statement.Parameters.First().Key));
        }

        [Test]
        public void Select_Count_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Count();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Scalar);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT COUNT(*) FROM [{0}] {1}", TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Skip_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Skip(10);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY [id]) AS [__RowNumber__] FROM [{0}] {1}) AS {1} WHERE [__RowNumber__] >= 11",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Skip_Projection_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Skip(10);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            select.Projection = new List<SelectProjection>
                                    {
                                        new SelectProjection {Projection = Projection.Create.Field("Name")},
                                        new SelectProjection {Projection = Projection.Create.Field("Age")}
                                    };
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT [name], [age] FROM (SELECT [name], [age], ROW_NUMBER() OVER (ORDER BY [name], [age]) AS [__RowNumber__] FROM [{0}] {1}) AS {1} WHERE [__RowNumber__] >= 11",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Skip_And_Take_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Skip(10).Take(10);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY [id]) AS [__RowNumber__] FROM [{0}] {1}) AS {1} WHERE [__RowNumber__] BETWEEN 11 AND 20",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Skip_And_Take_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Skip(10).Take(10).Where(x => x.Active);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(true);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY [id]) AS [__RowNumber__] FROM [{0}] {1} WHERE ([active] = @{2})) AS {1} WHERE [__RowNumber__] BETWEEN 11 AND 20",
                                                    TableName1, select.From.Alias, statement.Parameters.First().Key));
        }

        [Test]
        public void Select_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Where(y => y.Name == "hello");
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual("hello");
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [{0}] {1} WHERE ([name] = @{2})", TableName1, select.From.Alias, statement.Parameters.First().Key));
        }

        [Test]
        public void Select_Intersect_Test()
        {
            var query1 = MockQueryable<Entity>.Create(TableName1);
            var query2 = MockQueryable<Entity>.Create(TableName2);
            query1.Intersect(query2, x => x.Name, x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query1.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [{0}] {1} WHERE EXISTS (SELECT TOP (1) [name], [age] FROM [{2}] {3} WHERE (([name] = [{1}].[name]) AND ([age] = [{1}].[age])))", 
                                                    TableName1, 
                                                    select.From.Alias,
                                                    TableName2,
                                                    select.SetOperatons[0].Select.From.Alias));
        }

        [Test]
        public void Select_Intersect_Where_Test()
        {
            var query1 = MockQueryable<Entity>.Create(TableName1);
            var query2 = MockQueryable<Entity>.Create(TableName2);
            query1.Where(x => x.Active).Intersect(query2, x => x.Name.ToUpper(), x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query1.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(true);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [{0}] {1} WHERE ([active] = @{3}) AND EXISTS (SELECT TOP (1) [UPPER([name])], [age] FROM [{2}] {4} WHERE ((UPPER([name]) = UPPER([{1}].[name])) AND ([age] = [{1}].[age])))",
                                                    TableName1,
                                                    select.From.Alias,
                                                    TableName2,
                                                    statement.Parameters.First().Key,
                                                    select.SetOperatons[0].Select.From.Alias));
        }

        [Test]
        public void Select_Except_Test()
        {
            var query1 = MockQueryable<Entity>.Create(TableName1);
            var query2 = MockQueryable<Entity>.Create(TableName2);
            query1.Except(query2, x => x.Name, x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query1.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [{0}] {1} WHERE NOT EXISTS (SELECT TOP (1) [name], [age] FROM [{2}] {3} WHERE (([name] = [{1}].[name]) AND ([age] = [{1}].[age])))",
                                                    TableName1,
                                                    select.From.Alias,
                                                    TableName2,
                                                    select.SetOperatons[0].Select.From.Alias));
        }

        [Test]
        public void Select_Except_Where_Test()
        {
            var query1 = MockQueryable<Entity>.Create(TableName1);
            var query2 = MockQueryable<Entity>.Create(TableName2);
            query1.Where(x => x.Active).Except(query2, x => x.Name.ToUpper(), x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query1.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(true);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [{0}] {1} WHERE ([active] = @{3}) AND NOT EXISTS (SELECT TOP (1) [UPPER([name])], [age] FROM [{2}] {4} WHERE ((UPPER([name]) = UPPER([{1}].[name])) AND ([age] = [{1}].[age])))",
                                                    TableName1,
                                                    select.From.Alias,
                                                    TableName2,
                                                    statement.Parameters.First().Key,
                                                    select.SetOperatons[0].Select.From.Alias));
        }

        [Test]
        public void Select_Paged_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Where(y => y.Name == "hello").Skip(10).Take(10);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual("hello");
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY [id]) AS [__RowNumber__] FROM [{0}] {1} WHERE ([name] = @{2})) AS {1} WHERE [__RowNumber__] BETWEEN 11 AND 20",
                TableName1, select.From.Alias, statement.Parameters.First().Key));
        }

        [Test]
        public void Select_Order_By_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.OrderBy(y => y.Name).OrderByDescending(y => y.Values["CompanyName"]);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [{0}] {1} ORDER BY [name] ASC, [companyname] DESC", TableName1, select.From.Alias));
        }

        [Test]
        public void Select_Paged_Order_By_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.OrderBy(y => y.Name).OrderByDescending(y => y.Values["CompanyName"]).Skip(10).Take(10);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            
            statement.Result.ShouldEqual(Statement.ResultType.Multiple);
            statement.Parameters.Count.ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY [name] ASC, [companyname] DESC) AS [__RowNumber__] FROM [{0}] {1}) AS {1} WHERE [__RowNumber__] BETWEEN 11 AND 20",
                                                    TableName1, select.From.Alias));
        }

        [Test]
        public void Chained_Union_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Union(MockQueryable<Entity>.Create(TableName2).Union(MockQueryable<Entity>.Create(TableName3)));
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [XLIST_3] {0} UNION SELECT * FROM [XLIST_2] {1} UNION SELECT * FROM [XLIST_1] {2}",
                                                  select.From.Queries[0].From.Queries[0].From.Alias,
                                                  select.From.Queries[0].From.Queries[1].From.Alias,
                                                  select.From.Queries[1].From.Alias));
        }

        [Test]
        public void Nested_Union_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Union(MockQueryable<Entity>.Create(TableName2).Union(MockQueryable<Entity>.Create(TableName3)));
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT * FROM [XLIST_3] {0} UNION SELECT * FROM [XLIST_2] {1} UNION SELECT * FROM [XLIST_1] {2}",
                                                  select.From.Queries[0].From.Queries[0].From.Alias,
                                                  select.From.Queries[0].From.Queries[1].From.Alias,
                                                  select.From.Queries[1].From.Alias));
        }

        [Test]
        public void Chained_Union_Top_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Union(MockQueryable<Entity>.Create(TableName2).Take(1).Union(MockQueryable<Entity>.Create(TableName3).Take(2)));
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT TOP (2) * FROM [XLIST_3] {0} UNION SELECT TOP (1) * FROM [XLIST_2] {1} UNION SELECT * FROM [XLIST_1] {2}",
                                                    select.From.Queries[0].From.Queries[0].From.Alias,
                                                    select.From.Queries[0].From.Queries[1].From.Alias,
                                                    select.From.Queries[1].From.Alias));
        }

        [Test]
        public void Nested_Union_Top_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Union(MockQueryable<Entity>.Create(TableName2).Take(1).Union(MockQueryable<Entity>.Create(TableName3)).Take(3)).Take(5);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT TOP (5) * FROM (SELECT TOP (3) * FROM (SELECT * FROM [XLIST_3] {0} UNION SELECT TOP (1) * FROM [XLIST_2] {1}) AS {2} UNION SELECT * FROM [XLIST_1] {3}) AS {4}",
                                                    select.From.Queries[0].From.Queries[0].From.Alias,
                                                    select.From.Queries[0].From.Queries[1].From.Alias,
                                                    select.From.Queries[0].From.Alias,
                                                    select.From.Queries[1].From.Alias,
                                                    select.From.Alias));
        }

        [Test]
        public void Nested_Union_Projection_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Union(MockQueryable<Entity>.Create(TableName2).Take(1).Union(MockQueryable<Entity>.Create(TableName3)).Take(3)).Take(5);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;
            select.Projection = new List<SelectProjection>
                                    {
                                        new SelectProjection {Projection = Projection.Create.Field("Name")},
                                        new SelectProjection {Projection = Projection.Create.Field("Age")}
                                    };
            var statement = SelectWriter<Entity>.CreateStatement(select, Map);

            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual(string.Format("SELECT TOP (5) [name], [age] FROM (SELECT TOP (3) [name], [age] FROM (SELECT [name], [age] FROM [XLIST_3] {0} UNION SELECT TOP (1) [name], [age] FROM [XLIST_2] {1}) AS {2} UNION SELECT [name], [age] FROM [XLIST_1] {3}) AS {4}",
                                                    select.From.Queries[0].From.Queries[0].From.Alias,
                                                    select.From.Queries[0].From.Queries[1].From.Alias,
                                                    select.From.Queries[0].From.Alias,
                                                    select.From.Queries[1].From.Alias,
                                                    select.From.Alias));
        }
    }
}
