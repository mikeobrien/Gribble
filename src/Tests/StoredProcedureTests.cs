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
    public class StoredProcedureTests
    {
        private static readonly TestDatabase Database =
            new TestDatabase("[id] [int] IDENTITY(1,1) NOT NULL, [name] [varchar] (500) NULL, [hide] [bit] NULL, [timestamp] [datetime] NULL",
                             10, "name, hide, [timestamp]", "'oh hai', 0, GETDATE()");

        public class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> Values { get; set; }
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

        public static EntityMappingCollection MappingCollection = new EntityMappingCollection(new IClassMap[] { new TestClassMap() });
        public static IProfiler Profiler = new ConsoleProfiler();
        public IStoredProcedure StoredProcedure;

        [TestFixtureSetUp]
        public void Setup()
        {
            Database.SetUp();
            Database.CreateTables();
            Database.ExecuteNonQuery("CREATE PROCEDURE GetAll AS BEGIN SELECT * FROM {0} END", Database.FirstTable.Name);
            Database.ExecuteNonQuery("CREATE PROCEDURE GetOne @Id int AS BEGIN SELECT TOP 1 * FROM {0} WHERE Id=@Id END", Database.FirstTable.Name);
            Database.ExecuteNonQuery("CREATE PROCEDURE GetCount AS BEGIN SELECT COUNT(*) FROM {0} END", Database.FirstTable.Name);
            Database.ExecuteNonQuery("CREATE PROCEDURE DeleteOne @Id int AS BEGIN DELETE FROM {0} WHERE Id=@Id END", Database.FirstTable.Name);
            StoredProcedure = Gribble.StoredProcedure.Create(Database.Connection, MappingCollection, profiler: Profiler);
        }

        [TestFixtureTearDown]
        public void TearDown() { Database.TearDown(); }

        [SetUp]
        public void TestSetup() { Database.CreateTables(); }

        [Test]
        public void Get_All_Test()
        {
            var results = StoredProcedure.ExecuteMany<Entity>("GetAll").ToList();
            results.Count().ShouldEqual(10);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > -1).ShouldEqual(true);
            results.First().Values.Count.ShouldEqual(2);
            ((bool)results.First().Values["hide"]).ShouldEqual(false);
            ((DateTime)results.First().Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Get_One_Test()
        {
            var result = StoredProcedure.ExecuteSingle<Entity>("GetOne", new Dictionary<string, object> { { "Id", 5 } });
            result.ShouldNotBeNull();
            result.Name.Length.ShouldBeGreaterThan(3);
            result.Id.ShouldEqual(5);
            result.Values.Count.ShouldEqual(2);
            ((bool)result.Values["hide"]).ShouldEqual(false);
            ((DateTime)result.Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Get_Scalar_Test()
        {
            var result = StoredProcedure.ExecuteScalar<int>("GetCount");
            result.ShouldBeGreaterThan(8);
        }

        [Test]
        public void Get_Non_Query_Test()
        {
            StoredProcedure.Execute("DeleteOne", new Dictionary<string, object> { { "Id", 6 } });

            var result = StoredProcedure.ExecuteScalar<int>("GetCount");
            result.ShouldEqual(9);
        }
    }
}
