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

namespace Tests
{
    [TestFixture]
    public class LoaderTests
    {
        private static readonly TestDatabase Database = new TestDatabase();

        static LoaderTests()
        {
            const int records = 10;
            const string columnSchena = "[id] [int] IDENTITY(1,1) NOT NULL, [name] [varchar] (500) NULL, [age] [int] NULL, [type] [varchar] (50) NULL, [create_date] [datetime] NULL, [uid] [uniqueidentifier] NOT NULL";
            const string dataColumns = "name, age, [create_date], [uid], [type]";
            const string data = "'oh hai', 0, GETDATE(), NEWID(), 'U'";

            Database.AddTable(columnSchena + ", [a] [int] NULL", records, dataColumns, data);
            Database.AddTable(columnSchena + ", [a] [int] NULL, [b] [varchar] NULL", records, dataColumns, data);
            Database.AddTable(columnSchena + ", [a] [int] NULL, [b] [varchar] NULL, [c] [bit] NULL", records, dataColumns, data);
        }

        public class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }

        public class EntityMap : ClassMap<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Age).Column("age");
                Map(x => x.Values).Dynamic();
            }
        }

        private static readonly EntityMapping Map = new EntityMapping(new EntityMap());

        public static IProfiler Profiler = new ConsoleProfiler();

        [TestFixtureSetUp]
        public void Setup() { Database.SetUp(); }

        [TestFixtureTearDown]
        public void TearDown() { Database.TearDown(); }

        [SetUp]
        public void TestSetup() { Database.CreateTables(); }

        private static object GetResult(MockQueryable<Entity> query)
        {
            var statement = SelectWriter<Entity>.CreateStatement(SelectVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name), Map);
            var command = Command.Create(statement, Profiler);
            var loader = new Loader<Entity>(command, Map);
            return loader.Execute(new ConnectionManager(Database.Connection, TimeSpan.FromMinutes(5)));
        }

        [Test]
        public void should_not_return_null_comparison_values()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name=NULL WHERE id > 5", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Where(x => x.Name != "ed");
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(10);
            results.Count(x => x.Name == "oh hai").ShouldEqual(5);
            results.Count(x => x.Name == null).ShouldEqual(5);
        }

        [Test]
        public void Select_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Top_Count_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Take(2);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Top_Percent_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.TakePercent(50);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(5);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Randomize_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Randomize();
            var result1 = GetResult(query);
            var result2 = GetResult(query);
            result1.ShouldImplement<IEnumerable<Entity>>();
            result2.ShouldImplement<IEnumerable<Entity>>();
            var results1 = (((IEnumerable<Entity>)result1).ToList()).ToList();
            var results2 = (((IEnumerable<Entity>)result2).ToList()).ToList();
            results1.Count().ShouldEqual(10);
            results2.Count().ShouldEqual(10);
            results1.Zip(results2, (x,y) => x.Id == y.Id).Any(x => !x).ShouldEqual(true);
        }

        [Test]
        public void should_return_distinct_results()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=22 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=22 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=33 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=33 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=22 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=55 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=55 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=66 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Distinct(x => x.Name.ToUpper()).Distinct(x => x.Age);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(6);
            results.All(x => x.Name.Length >= 2).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);

            results.Exists(x => x.Name == "Tom" && x.Age == 31).ShouldEqual(true);
            results.Exists(x => x.Name == "Tom" && x.Age == 22).ShouldEqual(true);
            results.Exists(x => x.Name == "Dick" && x.Age == 33).ShouldEqual(true);
            results.Exists(x => x.Name == "Dick" && x.Age == 22).ShouldEqual(true);
            results.Exists(x => x.Name == "Harry" && x.Age == 55).ShouldEqual(true);
            results.Exists(x => x.Name == "Harry" && x.Age == 66).ShouldEqual(true);
        }

        [Test]
        public void should_return_distinct_results_with_take_and_skip()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=22 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=22 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=33 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=33 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=22 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=55 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=55 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=66 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Distinct(x => x.Name.ToUpper()).Distinct(x => x.Age).Skip(3).Take(4);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(3);
            results.All(x => x.Name.Length >= 2).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);

            results.Exists(x => x.Name == "Dick" && x.Age == 22).ShouldEqual(true);
            results.Exists(x => x.Name == "Harry" && x.Age == 55).ShouldEqual(true);
            results.Exists(x => x.Name == "Harry" && x.Age == 66).ShouldEqual(true);
        }

        [Test]
        public void should_return_distinct_results_with_order_field_ascending()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=22 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=22 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=33 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=33 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=22 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=55 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=55 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=66 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Distinct(x => x.Name).Distinct(x => x.Age, x => x.Id, Order.Ascending);
            var results = ((IEnumerable<Entity>)GetResult(query)).ToList();
            results.Count().ShouldEqual(6);

            results.Exists(x => x.Name == "Tom" && x.Age == 31 && x.Id == 1).ShouldEqual(true);
            results.Exists(x => x.Name == "Tom" && x.Age == 22 && x.Id == 3).ShouldEqual(true);
            results.Exists(x => x.Name == "Dick" && x.Age == 33 && x.Id == 5).ShouldEqual(true);
            results.Exists(x => x.Name == "Dick" && x.Age == 22 && x.Id == 7).ShouldEqual(true);
            results.Exists(x => x.Name == "Harry" && x.Age == 55 && x.Id == 8).ShouldEqual(true);
            results.Exists(x => x.Name == "Harry" && x.Age == 66 && x.Id == 10).ShouldEqual(true);
        }

        [Test]
        public void should_return_distinct_results_with_order_field_descending()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=22 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=22 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=33 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=33 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=22 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=55 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=55 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=66 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Distinct(x => x.Name).Distinct(x => x.Age, x => x.Id, Order.Descending);
            var results = ((IEnumerable<Entity>)GetResult(query)).ToList();
            results.Count().ShouldEqual(6);

            results.Exists(x => x.Name == "Tom" && x.Age == 31 && x.Id == 2).ShouldEqual(true);
            results.Exists(x => x.Name == "Tom" && x.Age == 22 && x.Id == 4).ShouldEqual(true);
            results.Exists(x => x.Name == "Dick" && x.Age == 33 && x.Id == 6).ShouldEqual(true);
            results.Exists(x => x.Name == "Dick" && x.Age == 22 && x.Id == 7).ShouldEqual(true);
            results.Exists(x => x.Name == "Harry" && x.Age == 55 && x.Id == 9).ShouldEqual(true);
            results.Exists(x => x.Name == "Harry" && x.Age == 66 && x.Id == 10).ShouldEqual(true);
        }

        [Test]
        public void should_return_duplicates()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=64 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=52 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=68 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=73 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=38 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=52 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=85 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=78 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=26 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Duplicates(x => x.Name);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(7);
            results.Count(x => x.Name == "Tom").ShouldEqual(3);
            results.Count(x => x.Name == "Dick").ShouldEqual(2);
            results.Count(x => x.Name == "Harry").ShouldEqual(2);
        }

        [Test]
        public void should_return_duplicates_of_precidence()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=64 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=52 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=68 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=73 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=38 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=52 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=85 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=78 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=26 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Duplicates(x => x.Name, x => x.Age > 50, Order.Ascending);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(7);
            results.All(x => x.Age > 50).ShouldBeTrue();
            results.Count(x => x.Name == "Tom").ShouldEqual(3);
            results.Count(x => x.Name == "Dick").ShouldEqual(2);
            results.Count(x => x.Name == "Harry").ShouldEqual(2);
        }

        [Test]
        public void should_return_duplicates_by_sort()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=64 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=52 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=68 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=73 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=38 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=52 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=85 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=78 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=26 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Duplicates(x => x.Name, x => x.Age, Order.Ascending);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(7);
            results.All(x => x.Age > 50).ShouldBeTrue();
            results.Count(x => x.Name == "Tom").ShouldEqual(3);
            results.Count(x => x.Name == "Dick").ShouldEqual(2);
            results.Count(x => x.Name == "Harry").ShouldEqual(2);
        }

        [Test]
        public void should_return_duplicates_by_sort_descending()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=31 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=64 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=52 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=88 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=83 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=38 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=52 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=85 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=78 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=26 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Duplicates(x => x.Name, x => x.Age, Order.Descending);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(7);
            results.All(x => x.Age < 80).ShouldBeTrue();
            results.Count(x => x.Name == "Tom").ShouldEqual(3);
            results.Count(x => x.Name == "Dick").ShouldEqual(2);
            results.Count(x => x.Name == "Harry").ShouldEqual(2);
        }

        [Test]
        public void should_return_duplicates_by_double_sort()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=84 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=84 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=42 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=42 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=83 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=43 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=85 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=85 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=48 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=48 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Duplicates(x => x.Name, x => x.Age, Order.Ascending, x => x.Id, Order.Descending);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(7);
            results.Count(x => x.Name == "Dick").ShouldEqual(1);
            var subResults = results.Where(x => x.Name == "Tom").ToList();
            subResults.Count().ShouldEqual(3);
            subResults[0].Age.ShouldEqual(42);
            subResults[0].Id.ShouldEqual(3);
            subResults[1].Age.ShouldEqual(84);
            subResults[1].Id.ShouldEqual(2);
            subResults[2].Age.ShouldEqual(84);
            subResults[2].Id.ShouldEqual(1);
            subResults = results.Where(x => x.Name == "Harry").ToList();
            subResults.Count().ShouldEqual(3);
            subResults[0].Age.ShouldEqual(48);
            subResults[0].Id.ShouldEqual(9);
            subResults[1].Age.ShouldEqual(85);
            subResults[1].Id.ShouldEqual(8);
            subResults[2].Age.ShouldEqual(85);
            subResults[2].Id.ShouldEqual(7);
        }

        [Test]
        public void should_return_duplicates_by_double_sort_descending()
        {
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=84 WHERE id = 1", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=84 WHERE id = 2", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=42 WHERE id = 3", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Tom', age=42 WHERE id = 4", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=83 WHERE id = 5", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Dick', age=43 WHERE id = 6", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=85 WHERE id = 7", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=85 WHERE id = 8", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=48 WHERE id = 9", Database.FirstTable.Name);
            Database.ExecuteNonQuery("UPDATE [{0}] SET name='Harry', age=48 WHERE id = 10", Database.FirstTable.Name);
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Duplicates(x => x.Name, x => x.Age, Order.Descending, x => x.Id, Order.Ascending);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(7);
            results.Count(x => x.Name == "Dick").ShouldEqual(1);
            var subResults = results.Where(x => x.Name == "Tom").ToList();
            subResults.Count().ShouldEqual(3);
            subResults[0].Age.ShouldEqual(84);
            subResults[0].Id.ShouldEqual(2);
            subResults[1].Age.ShouldEqual(42);
            subResults[1].Id.ShouldEqual(3);
            subResults[2].Age.ShouldEqual(42);
            subResults[2].Id.ShouldEqual(4);
            subResults = results.Where(x => x.Name == "Harry").ToList();
            subResults.Count().ShouldEqual(3);
            subResults[0].Age.ShouldEqual(85);
            subResults[0].Id.ShouldEqual(8);
            subResults[1].Age.ShouldEqual(48);
            subResults[1].Id.ShouldEqual(9);
            subResults[2].Age.ShouldEqual(48);
            subResults[2].Id.ShouldEqual(10);
        }

        [Test]
        public void Select_First_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.First();
            var result = GetResult(query);
            result.ShouldBeType(typeof(Entity));
            var entity = (Entity)result;
            entity.Name.Length.ShouldBeGreaterThan(3);
            entity.Id.ShouldBeGreaterThan(0);
            entity.Values.Count.ShouldBeGreaterThan(2);
            ((string)entity.Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)entity.Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_First_Or_Default_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.FirstOrDefault();
            var result = GetResult(query);
            result.ShouldBeType(typeof(Entity));
            var entity = (Entity)result;
            entity.Name.Length.ShouldBeGreaterThan(3);
            entity.Id.ShouldBeGreaterThan(0);
            entity.Values.Count.ShouldBeGreaterThan(2);
            ((string)entity.Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)entity.Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Count_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Count();
            var result = GetResult(query);
            result.ShouldBeType(typeof(int));
            var count = (int)result;
            count.ShouldBeGreaterThan(3);
        }

        [Test]
        public void should_return_any()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Any();
            var result = GetResult(query);
            result.ShouldBeType(typeof(bool));
            ((bool)result).ShouldBeTrue();
        }

        [Test]
        public void should_return_any_with_true_predicate()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Any(x => x.Id >= 5);
            var result = GetResult(query);
            result.ShouldBeType(typeof(bool));
            ((bool)result).ShouldBeTrue();
        }
        [Test]
        public void should_return_any_with_false_predicate()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Any(x => x.Id >= 15);
            var result = GetResult(query);
            result.ShouldBeType(typeof(bool));
            ((bool)result).ShouldBeFalse();
        }

        [Test]
        public void Select_Skip_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Skip(2);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Skip_And_Take_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Skip(2).Take(2);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Where(y => ((string)y.Values["type"]).Trim() == "U");
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Paged_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Where(y => ((string)y.Values["type"]).Trim() == "U").Skip(2).Take(2);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Order_By_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.OrderBy(y => y.Name).OrderByDescending(y => y.Values["type"]);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Paged_Order_By_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.OrderBy(y => y.Name).OrderByDescending(y => y.Values["type"]).Skip(2).Take(2);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((string)results.First().Values["type"]).Trim().ShouldEqual("U");
            ((DateTime)results.First().Values["create_date"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Except_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Except(MockQueryable<Entity>.Create(Database.SecondTable.Name).Where(x => x.Id <= 5), x => x.Id);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(5);
            results.All(x => x.Id > 5).ShouldEqual(true);
        }

        [Test]
        public void Select_Except_Union_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Except(MockQueryable<Entity>.Create(Database.SecondTable.Name).Union(MockQueryable<Entity>.Create(Database.ThirdTable.Name)).Where(x => x.Id <= 5), x => x.Id);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(5);
            results.All(x => x.Id > 5).ShouldEqual(true);
        }

        [Test]
        public void Select_Intersect_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Intersect(MockQueryable<Entity>.Create(Database.SecondTable.Name).Where(x => x.Id <= 5), x => x.Id);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(5);
            results.All(x => x.Id <= 5).ShouldEqual(true);
        }

        [Test]
        public void Select_Intersect_Union_Test()
        {
            var query = MockQueryable<Entity>.Create(Database.FirstTable.Name);
            query.Intersect(MockQueryable<Entity>.Create(Database.SecondTable.Name).Union(MockQueryable<Entity>.Create(Database.ThirdTable.Name)).Where(x => x.Id <= 5), x => x.Id);
            var result = GetResult(query);
            result.ShouldImplement<IEnumerable<Entity>>();
            var results = ((IEnumerable<Entity>)result).ToList();
            results.Count().ShouldEqual(5);
            results.All(x => x.Id <= 5).ShouldEqual(true);
        }
    }
}
