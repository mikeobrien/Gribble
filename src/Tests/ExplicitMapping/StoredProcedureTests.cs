using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gribble;
using Gribble.Mapping;
using NUnit.Framework;
using Should;

namespace Tests.ExplicitMapping
{
    [TestFixture]
    public class StoredProcedureTests
    {
        private static readonly TestDatabase Database =
            new TestDatabase("[id] [int] IDENTITY(1,1) NOT NULL, [name] [varchar] (500) NULL, [hide] [bit] NULL, [timestamp] [datetime] NULL",
                             10, "name, hide, [timestamp]", "'oh hai', 0, GETDATE()");

        public class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        public class TestClassMap : ClassMap<Entity>
        {
            public TestClassMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Values).Dynamic();
            }
        }

        public class NoIdEntity
        {
            public string Name { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        public class TestNoIdClassMap : ClassMap<NoIdEntity>
        {
            public TestNoIdClassMap()
            {
                Map(x => x.Name).Column("name");
                Map(x => x.Values).Dynamic();
            }
        }

        public static EntityMappingCollection MappingCollection = new EntityMappingCollection(new IClassMap[] { new TestClassMap(), new TestNoIdClassMap() });
        public static IProfiler Profiler = new ConsoleProfiler();
        public IStoredProcedure StoredProcedure;

        [OneTimeSetUp]
        public void Setup()
        {
            Database.SetUp();
            Database.CreateTables();
            Database.ExecuteNonQuery("CREATE PROCEDURE ReturnValue AS BEGIN RETURN 42 END", Database.FirstTable.Name);
            Database.ExecuteNonQuery("CREATE PROCEDURE GetAll AS BEGIN SELECT * FROM {0} END", Database.FirstTable.Name);
            Database.ExecuteNonQuery("CREATE PROCEDURE GetOne @Id int AS BEGIN SELECT TOP 1 * FROM {0} WHERE Id=@Id END", Database.FirstTable.Name);
            Database.ExecuteNonQuery("CREATE PROCEDURE GetCount AS BEGIN SELECT COUNT(*) FROM {0} END", Database.FirstTable.Name);
            Database.ExecuteNonQuery("CREATE PROCEDURE DeleteOne @Id int AS BEGIN DELETE FROM {0} WHERE Id=@Id END", Database.FirstTable.Name);
            Database.ExecuteNonQuery("CREATE PROCEDURE Echo @Value int AS BEGIN SELECT @Value END", Database.FirstTable.Name);
            StoredProcedure = Gribble.StoredProcedure.Create(Database.Connection, MappingCollection, profiler: Profiler);
        }

        [OneTimeTearDown]
        public void TearDown() { Database.TearDown(); }

        [SetUp]
        public void TestSetup() { Database.CreateTables(); }

        [Test]
        public void should_get_multiple_results()
        {
            var results = StoredProcedure.ExecuteMany<Entity>("GetAll").ToList();
            results.Count().ShouldEqual(10);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > -1).ShouldEqual(true);

            var result = results.First();
            result.Values.Count.ShouldEqual(4);
            result.Values["hide"].ShouldEqual(false);
            result.Values["Id"].ShouldEqual(1);
            result.Values["Name"].ShouldEqual("oh hai");
            ((DateTime)result.Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }
        
        [Test]
        public void should_get_multiple_object_array_results()
        {
            var results = StoredProcedure.ExecuteMany<object[]>("GetAll").ToList();
            results.Count.ShouldEqual(10);
            var firstResult = results[0];
            firstResult[0].ShouldEqual(1);
            firstResult[1].ShouldEqual("oh hai");
        }
        
        [Test]
        public void should_get_multiple_dictionary_results()
        {
            var results = StoredProcedure.ExecuteMany<Dictionary<string, object>>("GetAll").ToList();
            results.Count.ShouldEqual(10);
            var firstResult = results[0];
            firstResult["id"].ShouldEqual(1);
            firstResult["name"].ShouldEqual("oh hai");
        }
        
        [Test]
        public void should_get_multiple_dictionary_interface_results()
        {
            var results = StoredProcedure.ExecuteMany<IDictionary<string, object>>("GetAll").ToList();
            results.Count.ShouldEqual(10);
            var firstResult = results[0];
            firstResult["id"].ShouldEqual(1);
            firstResult["name"].ShouldEqual("oh hai");
        }
        
        [Test]
        public void should_get_multiple_dictionary_extension_method_results()
        {
            var results = StoredProcedure.ExecuteDictionary("GetAll").ToList();
            results.Count.ShouldEqual(10);
            var firstResult = results[0];
            firstResult["id"].ShouldEqual(1);
            firstResult["name"].ShouldEqual("oh hai");
        }

        [Test]
        public void should_get_data_table()
        {
            var table = StoredProcedure.ExecuteDataTable("fark", "GetAll");
            table.TableName.ShouldEqual("fark");
            var rows = table.Rows.Cast<DataRow>().ToList();
            rows.Count.ShouldEqual(10);
            rows.All(x => ((string)x["Name"]).Length > 3).ShouldEqual(true);
            rows.All(x => (int)x["Id"] > -1).ShouldEqual(true);
            rows.First().ItemArray.Length.ShouldEqual(4);
            ((bool)rows.First()["hide"]).ShouldEqual(false);
            ((DateTime)rows.First()["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void should_get_data_set()
        {
            var tables = StoredProcedure.ExecuteDataSet("GetAll");
            tables.Tables.Count.ShouldEqual(1);
            var table = tables.Tables.Cast<DataTable>().First();
            table.TableName.ShouldEqual("Table");
            var rows = table.Rows.Cast<DataRow>().ToList();
            rows.Count.ShouldEqual(10);
            rows.All(x => ((string)x["Name"]).Length > 3).ShouldEqual(true);
            rows.All(x => (int)x["Id"] > -1).ShouldEqual(true);
            rows.First().ItemArray.Length.ShouldEqual(4);
            ((bool)rows.First()["hide"]).ShouldEqual(false);
            ((DateTime)rows.First()["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void should_get_multiple_results_without_an_id()
        {
            var results = StoredProcedure.ExecuteMany<NoIdEntity>("GetAll").ToList();
            results.Count().ShouldEqual(10);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            var result = results.First();
            result.Values.Count.ShouldEqual(4);
            ((int)result.Values["id"]).ShouldBeGreaterThan(-1);
            result.Values["hide"].ShouldEqual(false);
            result.Values["Name"].ShouldEqual("oh hai");
            ((DateTime)result.Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void should_get_one_result()
        {
            var result = StoredProcedure.ExecuteSingle<Entity>("GetOne", new { Id = 5 });
            result.ShouldNotBeNull();
            result.Name.Length.ShouldBeGreaterThan(3);
            result.Id.ShouldEqual(5);
            result.Values.Count.ShouldEqual(4);
            ((bool)result.Values["hide"]).ShouldEqual(false);
            result.Values["Id"].ShouldEqual(5);
            result.Values["Name"].ShouldEqual("oh hai");
            ((DateTime)result.Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void should_get_scalar_result()
        {
            var result = StoredProcedure.ExecuteScalar<int>("GetCount");
            result.ShouldBeGreaterThan(8);
        }

        [Test]
        public void should_get_async_scalar_result()
        {
            var result = StoredProcedure.ExecuteScalarAsync<int>("GetCount").Result;
            result.ShouldBeGreaterThan(8);
        }

        [Test]
        public void should_execute_non_query()
        {
            StoredProcedure.ExecuteNonQuery("DeleteOne", new { Id = 6 }).ShouldEqual(1);

            var result = StoredProcedure.ExecuteScalar<int>("GetCount");
            result.ShouldEqual(9);
        }

        [Test]
        public void should_execute_async_non_query()
        {
            StoredProcedure.ExecuteNonQuery("DeleteOne", new { Id = 6 }).ShouldEqual(1);

            var result = StoredProcedure.ExecuteScalarAsync<int>("GetCount").Result;
            result.ShouldEqual(9);
        }

        [Test]
        public void should_execute_non_query_return()
        {
            StoredProcedure.ExecuteReturn<int>("ReturnValue").ShouldEqual(42);
        }

        [Test]
        public void should_execute_async_non_query_return()
        {
            StoredProcedure.ExecuteReturnAsync<int>("ReturnValue").Result.ShouldEqual(42);
        }

        [Test]
        public void should_allow_case_insensitive_params()
        {
            var result = StoredProcedure.ExecuteScalar<int>("Echo", new { VALUE = 5 });
            result.ShouldEqual(5);
        }

        [Test]
        public void should_allow_nullable_params()
        {
            int? value = 5;
            StoredProcedure.ExecuteScalar<int?>("Echo", new { Value = value }).ShouldEqual(5); 
        }

        [Test]
        public void should_return_nullable_scalar()
        {
            int? value = null;
            StoredProcedure.ExecuteScalar<int?>("Echo", new { Value = value }).ShouldEqual(null);
        }
    }
}
