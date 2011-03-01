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
    public class TableTests
    {
        private static readonly TestDatabase Database = new TestDatabase();

        static TableTests()
        {
            const int records = 10;
            const string columnSchena = "[id] [int] IDENTITY(1,1) NOT NULL, [name] [nvarchar] (500) NULL, [hide] [bit] NULL, [timestamp] [datetime] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), [code] [int] DEFAULT 5, [uid] [uniqueidentifier] NOT NULL";
            const string dataColumns = "name, hide, [timestamp], [uid]";
            const string data = "'oh hai', 0, GETDATE(), NEWID()";

            Database.AddTable(columnSchena, records, dataColumns, data);
            Database.AddTable(columnSchena, records, dataColumns, data);
            Database.AddTable("[id] [int] IDENTITY(1,1) NOT NULL, [name] [nvarchar] (500) NULL, [hide] [bit] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), [code] [int] DEFAULT 5", 5, "name, hide", "'oh hai yo', 1");
            Database.AddTable("[id] [int] IDENTITY(1,1) NOT NULL, [name] [varchar] (500) NULL, [hide] [bit] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), [code] [int] DEFAULT 5", 5, "name, hide", "'oh hai yo', 1");
        }

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

        private Table<IdentityEntity> _identityTable1;
        private Table<GuidEntity> _guidTable1;
        private Table<IdentityEntity> _identityTable2;
        private Table<IdentityEntity> _identityTable3;

        [TestFixtureSetUp]
        public void Setup()
        {
            Database.SetUp();
            _identityTable1 = new Table<IdentityEntity>(Database.Connection, TimeSpan.FromMinutes(5), Database.FirstTable.Name, IdentityMap, true);
            _guidTable1 = new Table<GuidEntity>(Database.Connection, TimeSpan.FromMinutes(5), Database.SecondTable.Name, GuidMap, true);
            _identityTable2 = new Table<IdentityEntity>(Database.Connection, TimeSpan.FromMinutes(5), Database.ThirdTable.Name, IdentityMap, true);
            _identityTable3 = new Table<IdentityEntity>(Database.Connection, TimeSpan.FromMinutes(5), Database.FourthTable.Name, IdentityMap, true);
        }

        [TestFixtureTearDown]
        public void TearDown() { Database.TearDown(); }

        [SetUp]
        public void TestSetup() { Database.CreateTables(); }

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
        public void Delete_Entity_Test()
        {
            var count = _identityTable1.Count();
            _identityTable1.Where(x => x.Id == 4).Count().ShouldEqual(1);
            _identityTable1.Delete(new IdentityEntity { Id = 4 });
            _identityTable1.Where(x => x.Id == 4).Count().ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(count - 1);
        }

        [Test]
        public void Delete_Single_By_Query_Test()
        {
            var count = _identityTable1.Count();
            _identityTable1.Where(x => x.Id == 8).Count().ShouldEqual(1);
            _identityTable1.Delete(x => x.Id == 8);
            _identityTable1.Where(x => x.Id == 8).Count().ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(count - 1);
        }

        [Test]
        public void Delete_Many_By_Query_Test()
        {
            Database.CreateTables();
            var totalCount = _identityTable1.Count();
            var setCount = _identityTable1.Where(x => x.Id > 8).Count();
            _identityTable1.DeleteMany(x => x.Id > 8);
            _identityTable1.Where(x => x.Id > 8).Count().ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(totalCount - setCount);
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
        public void Copy_Into_Existing_Test()
        {
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", Database.FirstTable.Name).ShouldEqual(10);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", Database.ThirdTable.Name).ShouldEqual(5);
            _identityTable1.Take(7).CopyTo(_identityTable2).Count().ShouldEqual(12);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", Database.FirstTable.Name).ShouldEqual(10);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", Database.ThirdTable.Name).ShouldEqual(12);
        }

        [Test]
        public void Copy_Into_New_Test()
        {
            const string newTable = "some_new_table";
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", Database.FirstTable.Name).ShouldEqual(10);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", Database.ThirdTable.Name).ShouldEqual(5);
            _identityTable1.Take(7).Union(_identityTable2).CopyTo(newTable).Count().ShouldEqual(12);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", Database.FirstTable.Name).ShouldEqual(10);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", Database.ThirdTable.Name).ShouldEqual(5);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.columns WHERE object_id=object_id('{0}') AND name = 'id'", newTable).ShouldEqual(1);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.columns WHERE object_id=object_id('{0}') AND name = 'name'", newTable).ShouldEqual(1);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.columns WHERE object_id=object_id('{0}') AND name = 'hide'", newTable).ShouldEqual(1);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.columns WHERE object_id=object_id('{0}') AND name = 'upc'", newTable).ShouldEqual(1);
            Database.ExecuteScalar<int>("SELECT COUNT(*) FROM sys.columns WHERE object_id=object_id('{0}') AND name = 'code'", newTable).ShouldEqual(1);
        }

        [Test]
        public void Copy_Into_Narrowing_Test()
        {
            Assert.Throws<StringColumnNarrowingException>(() => _identityTable1.Take(7).CopyTo(_identityTable3));
        }
    }
}
