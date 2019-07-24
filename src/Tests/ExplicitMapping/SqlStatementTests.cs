﻿using System;
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
    public class SqlStatementTests
    {
        private const string FirstBatch = "SELECT 1\r\nGO\r\n";
        private static readonly TestDatabase Database =
            new TestDatabase("[id] [int] IDENTITY(1,1) NOT NULL, [name] [varchar] (500) NULL, " + 
                    "[hide] [bit] NULL, [timestamp] [datetime] NULL",
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

        public static EntityMappingCollection MappingCollection = 
            new EntityMappingCollection(new IClassMap[]
            {
                new TestClassMap(), new TestNoIdClassMap()
            });
        public static IProfiler Profiler = new ConsoleProfiler();
        public ISqlStatement SqlStatement;

        [OneTimeSetUp]
        public void Setup()
        {
            Database.SetUp();
            Database.CreateTables();
            SqlStatement = Gribble.SqlStatement.Create(Database.Connection, MappingCollection, profiler: Profiler);
        }

        [OneTimeTearDown]
        public void TearDown() { Database.TearDown(); }

        [SetUp]
        public void TestSetup() { Database.CreateTables(); }

        [Test]
        public void should_get_multiple_results(
            [Values(FirstBatch, "")] string firstBatch)
        {
            var results = SqlStatement.ExecuteMany<Entity>($"{firstBatch}SELECT * FROM {Database.FirstTable.Name}").ToList();
            results.Count.ShouldEqual(10);
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
            var results = SqlStatement.ExecuteMany<object[]>($"SELECT id, name FROM {Database.FirstTable.Name}").ToList();
            results.Count.ShouldEqual(10);
            var firstResult = results[0];
            firstResult[0].ShouldEqual(1);
            firstResult[1].ShouldEqual("oh hai");
        }
        
        [Test]
        public void should_get_multiple_dictionary_results()
        {
            var results = SqlStatement.ExecuteMany<Dictionary<string, object>>(
                $"SELECT id, name FROM {Database.FirstTable.Name}").ToList();
            results.Count.ShouldEqual(10);
            var firstResult = results[0];
            firstResult["id"].ShouldEqual(1);
            firstResult["name"].ShouldEqual("oh hai");
        }
        
        [Test]
        public void should_get_multiple_dictionary_interface_results()
        {
            var results = SqlStatement.ExecuteMany<IDictionary<string, object>>(
                $"SELECT id, name FROM {Database.FirstTable.Name}").ToList();
            results.Count.ShouldEqual(10);
            var firstResult = results[0];
            firstResult["id"].ShouldEqual(1);
            firstResult["name"].ShouldEqual("oh hai");
        }

        [Test]
        public void should_get_data_table(
            [Values(FirstBatch, "")] string firstBatch)
        {
            var table = SqlStatement.ExecuteDataTable("fark", $"{firstBatch}SELECT * FROM {Database.FirstTable.Name}");
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
        public void should_get_data_set(
            [Values(FirstBatch, "")] string firstBatch)
        {
            var tables = SqlStatement.ExecuteDataSet($"{firstBatch}SELECT * FROM {Database.FirstTable.Name}");
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
        public void should_get_multiple_results_without_an_id(
            [Values(FirstBatch, "")] string firstBatch)
        {
            var results = SqlStatement.ExecuteMany<NoIdEntity>($"{firstBatch}SELECT * FROM {Database.FirstTable.Name}").ToList();
            results.Count.ShouldEqual(10);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            var result = results.First();
            result.Values.Count.ShouldEqual(4);
            ((int)result.Values["id"]).ShouldBeGreaterThan(-1);
            result.Values["hide"].ShouldEqual(false);
            result.Values["Name"].ShouldEqual("oh hai");
            ((DateTime)result.Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void should_get_one_result(
            [Values(FirstBatch, "")] string firstBatch)
        {
            var result = SqlStatement.ExecuteSingle<Entity>($"{firstBatch}SELECT TOP 1 * FROM {Database.FirstTable.Name} WHERE Id=@Id", new { Id = 5 });
            result.ShouldNotBeNull();
            result.Name.Length.ShouldBeGreaterThan(3);
            result.Id.ShouldEqual(5);
            result.Values.Count.ShouldEqual(4);
            result.Values["Id"].ShouldEqual(5);
            result.Values["Name"].ShouldEqual("oh hai");
            result.Values["hide"].ShouldEqual(false);
            ((DateTime)result.Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void should_get_scalar_result(
            [Values(FirstBatch, "")] string firstBatch)
        {
            var result = SqlStatement.ExecuteScalar<int>($"{firstBatch}SELECT COUNT(*) FROM {Database.FirstTable.Name}");
            result.ShouldBeGreaterThan(8);
        }

        [Test]
        public void should_get_scalar_async_result(
            [Values(FirstBatch, "")] string firstBatch)
        {
            var result = SqlStatement.ExecuteScalarAsync<int>(
                $"{firstBatch}SELECT COUNT(*) FROM {Database.FirstTable.Name}")
                .Result;
            result.ShouldBeGreaterThan(8);
        }

        [Test]
        public void should_execute_non_query(
            [Values(FirstBatch, "")] string firstBatch)
        {
            SqlStatement.ExecuteNonQuery($"{firstBatch}DELETE FROM {Database.FirstTable.Name} WHERE Id=@Id", new { Id = 6 }).ShouldEqual(1);

            var result = SqlStatement.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Database.FirstTable.Name}");
            result.ShouldEqual(9);
        }

        [Test]
        public void should_execute_non_query_async(
            [Values(FirstBatch, "")] string firstBatch)
        {
            SqlStatement.ExecuteNonQueryAsync(
                $"{firstBatch}DELETE FROM {Database.FirstTable.Name} WHERE Id=@Id", 
                new { Id = 6 })
                .Result
                .ShouldEqual(1);

            var result = SqlStatement.ExecuteScalar<int>($"SELECT COUNT(*) FROM {Database.FirstTable.Name}");
            result.ShouldEqual(9);
        }

        [Test]
        public void should_execute_multiple_batches()
        {
            var result = SqlStatement.ExecuteScalar<int>(
                $"INSERT INTO {Database.FirstTable.Name} (name) VALUES ('fark')" +
                $"\r\nGO\r\nSELECT COUNT(*) FROM {Database.FirstTable.Name} WHERE name = 'fark'");
            result.ShouldEqual(1);
        }

        [Test]
        public void should_allow_case_insensitive_params()
        {
            var result = SqlStatement.ExecuteScalar<int>("SELECT @Value", new { VALUE = 5 });
            result.ShouldEqual(5);
        }

        [Test]
        public void should_allow_nullable_params()
        {
            int? value = 5;
            SqlStatement.ExecuteScalar<int?>("SELECT @Value", new { Value = value }).ShouldEqual(5); 
        }

        [Test]
        public void should_return_nullable_scalar()
        {
            SqlStatement.ExecuteScalar<int?>("SELECT @Value", 
                new { Value = (int?)null }).ShouldEqual(null);
        }
    }
}
