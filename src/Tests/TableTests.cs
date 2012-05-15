using System;
using System.Collections.Generic;
using System.Linq;
using Gribble;
using Gribble.Mapping;
using Gribble.Model;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class TableTests
    {
        public class IdentityEntity
        {
            public IdentityEntity() { Values = new Dictionary<string, object>(); }
            public int Id { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }

        public class GuidEntity
        {
            public GuidEntity() { Values = new Dictionary<string, object>(); }
            public Guid Id { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }

        public class IdentityEntityMap : ClassMap<IdentityEntity>
        {
            public IdentityEntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Values).Dynamic();
            }
        }

        public class GuidEntityMap : ClassMap<GuidEntity>
        {
            public GuidEntityMap()
            {
                Id(x => x.Id).Column("uid").Generated();
                Map(x => x.Name).Column("name");
                Map(x => x.Values).Dynamic();
            }
        }

        private static readonly EntityMapping IdentityMap = new EntityMapping(new IdentityEntityMap());
        private static readonly EntityMapping GuidMap = new EntityMapping(new GuidEntityMap());

        private TestDatabase _database = new TestDatabase();
        private ITable<IdentityEntity> _identityTable1;
        private ITable<GuidEntity> _guidTable1;
        private ITable<IdentityEntity> _identityTable2;
        private ITable<IdentityEntity> _identityTable3;
        private IDatabase _gribbleDatabase;

        [SetUp]
        public void Setup()
        {
            const int records = 10;
            const string columnSchena = "[id] [int] IDENTITY(1,1) NOT NULL, [name] [nvarchar] (500) NULL, [hide] [bit] NULL, [timestamp] [datetime] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), [code] [int] DEFAULT 5, [uid] [uniqueidentifier] NOT NULL";
            const string dataColumns = "name, hide, [timestamp], [uid]";
            const string data = "'oh hai', 0, GETDATE(), NEWID()";

            _database = new TestDatabase();
            _database.AddTable(columnSchena, records, dataColumns, data);
            _database.AddTable(columnSchena, records, dataColumns, data);
            _database.AddTable("[id] [int] IDENTITY(1,1) NOT NULL, [name] [nvarchar] (500) NULL, [hide] [bit] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), [code] [int] DEFAULT 5", 5, "name, hide", "'oh hai yo', 1");
            _database.AddTable("[id] [int] IDENTITY(1,1) NOT NULL, [name] [varchar] (500) NULL, [hide] [bit] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), [code] [int] DEFAULT 5", 5, "name, hide", "'oh hai yo', 1");

            _database.SetUp();

            _identityTable1 = Table<IdentityEntity>.Create(_database.Connection, _database.FirstTable.Name, IdentityMap);
            _guidTable1 = Table<GuidEntity>.Create(_database.Connection, _database.SecondTable.Name, GuidMap);
            _identityTable2 = Table<IdentityEntity>.Create(_database.Connection, _database.ThirdTable.Name, IdentityMap);
            _identityTable3 = Table<IdentityEntity>.Create(_database.Connection, _database.FourthTable.Name, IdentityMap);
            _gribbleDatabase = Database.Create(_database.Connection, TimeSpan.FromMinutes(5));
            
            _database.CreateTables();
        }

        [TearDown]
        public void TearDown() { _database.TearDown(); }

        [Test]
        public void Get_Test()
        {
            var result = _identityTable1.Get(7);
            result.ShouldNotBeNull();
            result.Name.Length.ShouldBeGreaterThan(3);
            result.Id.ShouldEqual(7);
            result.Values.Count.ShouldBeGreaterThan(2);
            ((bool)result.Values["hide"]).ShouldEqual(false);
            ((DateTime)result.Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Test()
        {
            var results = _identityTable1.ToList();
            results.Count().ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((bool)results.First().Values["hide"]).ShouldEqual(false);
            ((DateTime)results.First().Values["timestamp"]).ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Select_Union()
        {
            var results = _identityTable1.Where(x => x.Id > 2).Union(_identityTable2.Where(x => x.Id > 3)).ToList();
            results.Count().ShouldEqual(10);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(0);
            ((bool)results.First().Values["hide"]).ShouldEqual(false);
        }

        [Test]
        public void Delete_By_Id_Test()
        {
            var count = _identityTable1.Count();
            _identityTable1.Count(x => x.Id == 4).ShouldEqual(1);
            _identityTable1.Delete(4);
            _identityTable1.Count(x => x.Id == 4).ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(count - 1);
        }

        [Test]
        public void Delete_Entity_Test()
        {
            var count = _identityTable1.Count();
            _identityTable1.Count(x => x.Id == 4).ShouldEqual(1);
            _identityTable1.Delete(new IdentityEntity { Id = 4 });
            _identityTable1.Count(x => x.Id == 4).ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(count - 1);
        }

        [Test]
        public void should_delete_one_by_predicate()
        {
            var count = _identityTable1.Count();
            _identityTable1.Count(x => x.Id == 8).ShouldEqual(1);
            _identityTable1.Delete(x => x.Id == 8);
            _identityTable1.Count(x => x.Id == 8).ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(count - 1);
        }

        [Test]
        public void should_delete_many_by_predicate()
        {
            _database.CreateTables();
            var totalCount = _identityTable1.Count();
            var setCount = _identityTable1.Count(x => x.Id > 8);
            _identityTable1.DeleteMany(x => x.Id > 8);
            _identityTable1.Count(x => x.Id > 8).ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(totalCount - setCount);
        }

        [Test]
        public void should_delete_many_by_query()
        {
            _database.CreateTables();
            var totalCount = _identityTable1.Count();
            var setCount = _identityTable1.Count(x => x.Id > 8);
            _identityTable1.DeleteMany(_identityTable1.Where(x => x.Id > 8));
            _identityTable1.Count(x => x.Id > 8).ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(totalCount - setCount);
        }

        [Test]
        public void should_delete_duplicates()
        {
            _database.CreateTables();
            _identityTable1.DeleteMany(_identityTable1.Duplicates(x => x.Name, x => x.Id != 5));
            _identityTable1.Count().ShouldEqual(1);
            _identityTable1.First().Id.ShouldEqual(5);
        }

        [Test]
        public void Update_Entity_Test()
        {
            var entity = _identityTable1.First(x => x.Id == 3);
            var hide = entity.Values["hide"];
            var timestamp = entity.Values["timestamp"];
            entity.Name = "Some new name.";
            var newUid = Guid.NewGuid();
            entity.Values["uid"] = newUid;
            _identityTable1.Update(entity);
            var newEntity = _identityTable1.First(x => x.Id == 3);
            newEntity.Name.ShouldEqual("Some new name.");
            newEntity.Values["timestamp"].ShouldEqual(timestamp);
            newEntity.Values["hide"].ShouldEqual(hide);
            newEntity.Values["uid"].ShouldEqual(newUid);
        }

        [Test]
        public void Insert_Identity_Entity_Test()
        {
            var entity = new IdentityEntity();
            entity.Name = "oh hai";
            entity.Values.Add("timestamp", DateTime.Now);
            entity.Values.Add("hide", true);
            entity.Values.Add("uid", Guid.NewGuid());
            _identityTable1.Insert(entity);
            entity.Id.ShouldBeGreaterThan(0);
            var newEntity = _identityTable1.First(x => x.Id == entity.Id);
            newEntity.Name.ShouldEqual("oh hai");
            newEntity.Values["timestamp"].ToString().ShouldEqual(entity.Values["timestamp"].ToString());
            newEntity.Values["hide"].ShouldEqual(entity.Values["hide"]);
            newEntity.Values["uid"].ShouldEqual(entity.Values["uid"]);
        }

        [Test]
        public void Insert_Entity_Test()
        {
            var entity = new GuidEntity();
            entity.Name = "oh hai";
            entity.Values.Add("timestamp", DateTime.Now);
            entity.Values.Add("hide", true);
            _guidTable1.Insert(entity);
            entity.Id.ShouldNotEqual(Guid.Empty);
            var newEntity = _guidTable1.First(x => x.Id == entity.Id);
            newEntity.Name.ShouldEqual("oh hai");
            newEntity.Values["timestamp"].ToString().ShouldEqual(entity.Values["timestamp"].ToString());
            newEntity.Values["hide"].ShouldEqual(entity.Values["hide"]);
        }

        [Test]
        public void should_insert_entity_with_null_value()
        {
            var entity = new GuidEntity();
            _guidTable1.Insert(entity);
            entity.Id.ShouldNotEqual(Guid.Empty);
            var newEntity = _guidTable1.First(x => x.Id == entity.Id);
            newEntity.Name.ShouldBeNull();
        }

        [Test]
        public void Copy_Into_Existing_Test()
        {
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", _database.FirstTable.Name).ShouldEqual(10);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", _database.ThirdTable.Name).ShouldEqual(5);
            _identityTable1.Take(7).CopyTo(_identityTable2).Count().ShouldEqual(12);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", _database.FirstTable.Name).ShouldEqual(10);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", _database.ThirdTable.Name).ShouldEqual(12);
        }

        [Test]
        public void Copy_Into_New_Test()
        {
            const string newTable = "some_new_table";
            _gribbleDatabase.AddNonClusteredIndex(_database.FirstTable.Name, new Index.Column("name"), new Index.Column("hide"));
            _gribbleDatabase.AddNonClusteredIndex(_database.FirstTable.Name, new Index.Column("upc"));
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", _database.FirstTable.Name).ShouldEqual(10);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", _database.ThirdTable.Name).ShouldEqual(5);
            _identityTable1.Take(7).Union(_identityTable2).CopyTo(newTable).Count().ShouldEqual(12);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", _database.FirstTable.Name).ShouldEqual(10);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", _database.ThirdTable.Name).ShouldEqual(5);
            var columns = _gribbleDatabase.GetColumns(newTable).ToList();
            columns.Count.ShouldEqual(5);
            columns.Count(x => x.Name == "id").ShouldEqual(1);
            columns.Count(x => x.Name == "name").ShouldEqual(1);
            columns.Count(x => x.Name == "hide").ShouldEqual(1);
            columns.Count(x => x.Name == "upc").ShouldEqual(1);
            columns.Count(x => x.Name == "code").ShouldEqual(1);

            var indexes = _gribbleDatabase.GetIndexes(newTable).ToList();
            indexes.Count.ShouldEqual(2);

            var index = indexes[1];
            index.Clustered.ShouldBeFalse();
            index.Columns.Count().ShouldEqual(1);
            index.Columns.First().Name.ShouldEqual("upc");
            index.Columns.First().Descending.ShouldBeFalse();
            index.Name.ShouldStartWith("IX_");
            index.Name.ShouldContain("_upc");
            index.PrimaryKey.ShouldBeFalse();
            index.Unique.ShouldBeFalse();

            index = indexes[0];
            index.Clustered.ShouldBeFalse();
            index.Columns.Count().ShouldEqual(2);
            index.Columns.First().Name.ShouldEqual("name");
            index.Columns.First().Descending.ShouldBeFalse();
            index.Columns.Last().Name.ShouldEqual("hide");
            index.Columns.Last().Descending.ShouldBeFalse();
            index.Name.ShouldStartWith("IX_");
            index.Name.ShouldContain("_name_hide");
            index.PrimaryKey.ShouldBeFalse();
            index.Unique.ShouldBeFalse();
        }

        [Test]
        public void Copy_Into_Narrowing_Test()
        {
            Assert.Throws<StringColumnNarrowingException>(() => _identityTable1.Take(7).CopyTo(_identityTable3));
        }
    }
}
