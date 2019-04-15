using System;
using System.Collections.Generic;
using System.Linq;
using Gribble;
using Gribble.Extensions;
using Gribble.Model;
using NUnit.Framework;
using Should;

namespace Tests.ImplicitMapping
{
    [TestFixture]
    public class ImplicitlyMappedTableTests
    {
        public class IdentityEntity
        {
            public IdentityEntity() { Values = new Dictionary<string, object>(); }
            public int Id { get; set; }
            public string Name { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }
        
        private TestDatabase _database = new TestDatabase();
        private ITable<IdentityEntity> _identityTable1;
        private ITable<IdentityEntity> _identityTable2;
        private ITable<IdentityEntity> _identityTable3;

        [SetUp]
        public void Setup()
        {
            const int records = 10;
            const string columnSchena = "[id] [int] IDENTITY(1,1) NOT NULL, [name] [nvarchar] (500) NULL, " + 
                "[hide] [bit] NULL, [timestamp] [datetime] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), " + 
                "[code] [int] DEFAULT 5";
            const string dataColumns = "name, hide, [timestamp]";
            const string data = "'oh hai', 0, GETDATE()";

            _database = new TestDatabase();
            _database.AddTable(columnSchena, records, dataColumns, data);
            _database.AddTable(columnSchena, records, dataColumns, data);
            _database.AddTable("[id] [int] IDENTITY(1,1) NOT NULL, [name] [nvarchar] (500) NULL, " + 
                "[hide] [bit] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), [code] [int] DEFAULT 5", 
                5, "name, hide", "'oh hai yo', 1");
            _database.AddTable("[id] [int] IDENTITY(1,1) NOT NULL, [name] [varchar] (500) NULL, " + 
                "[hide] [bit] NULL, [upc] [uniqueidentifier] DEFAULT NEWID(), [code] [int] DEFAULT 5", 
                5, "name, hide", "'oh hai yo', 1");

            _database.SetUp();

            _identityTable1 = Table<IdentityEntity>.Create(_database.Connection, _database.FirstTable.Name);
            _identityTable2 = Table<IdentityEntity>.Create(_database.Connection, _database.ThirdTable.Name);
            _identityTable3 = Table<IdentityEntity>.Create(_database.Connection, _database.FourthTable.Name);
            
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
            results.Count.ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((bool)results.First().Values["hide"]).ShouldEqual(false);
            ((DateTime)results.First().Values["timestamp"])
                .ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void Should_project_results_to_objects()
        {
            var results = _identityTable1.Select(x => new
            {
                x.Name,
                Month = x.Values["timestamp"].ToString().Substring(0, 3)
            }).ToList();
            results.Count.ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 0).ShouldBeTrue();
        }

        [Test]
        public void Should_select_values()
        {
            var results = _identityTable1.Select(x => x.Id).ToList();
            results.Count.ShouldBeGreaterThan(2);
            results.All(x => x > 0).ShouldBeTrue();
        }

        [Test]
        public void Should_select_projections()
        {
            var results = _identityTable1.Select(x => 
                x.Values["timestamp"].ToString().Substring(0, 3)).ToList();
            results.Count.ShouldBeGreaterThan(2);
            results.All(x => x.Length > 0).ShouldBeTrue();
        }

        [Test]
        public void should_select_records_with_nolock()
        {
            var results = Table<IdentityEntity>.Create(_database.Connection, 
                _database.FirstTable.Name, noLock: true).ToList();
            results.Count.ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((bool)results.First().Values["hide"]).ShouldEqual(false);
            ((DateTime)results.First().Values["timestamp"])
                .ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Test]
        public void should_union_records_with_nolock()
        {
            var results = Table<IdentityEntity>.Create(_database.Connection, 
                _database.FirstTable.Name, noLock: true)
                .Union(Table<IdentityEntity>.Create(_database.Connection, 
                _database.ThirdTable.Name, noLock: true)).ToList();
            results.Count.ShouldBeGreaterThan(2);
            results.All(x => x.Name.Length > 3).ShouldEqual(true);
            results.All(x => x.Id > 0).ShouldEqual(true);
            results.First().Values.Count.ShouldBeGreaterThan(2);
            ((bool)results.First().Values["hide"]).ShouldEqual(false);
        }

        [Test]
        public void Select_Union()
        {
            var results = _identityTable1.Where(x => x.Id > 2)
                .Union(_identityTable2.Where(x => x.Id > 3)).ToList();
            results.Count.ShouldEqual(10);
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
            _identityTable1.DeleteMany(x => x.Id > 8).ShouldEqual(2);
            _identityTable1.Count(x => x.Id > 8).ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(totalCount - setCount);
        }

        [Test]
        public void should_delete_many_by_query()
        {
            _database.CreateTables();
            var totalCount = _identityTable1.Count();
            var setCount = _identityTable1.Count(x => x.Id > 8);
            _identityTable1.DeleteMany(_identityTable1.Where(x => x.Id > 8)).ShouldEqual(2);
            _identityTable1.Count(x => x.Id > 8).ShouldEqual(0);
            _identityTable1.Count().ShouldEqual(totalCount - setCount);
        }

        [Test]
        public void should_delete_duplicates()
        {
            _database.CreateTables();
            _identityTable1.DeleteMany(_identityTable1.Duplicates(
                x => x.Name, x => x.Id != 5, Order.Ascending)).ShouldEqual(9);
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
            _identityTable1.Update(entity);
            var newEntity = _identityTable1.First(x => x.Id == 3);
            newEntity.Name.ShouldEqual("Some new name.");
            newEntity.Values["timestamp"].ShouldEqual(timestamp);
            newEntity.Values["hide"].ShouldEqual(hide);
        }

        [Test]
        public void Insert_Identity_Entity_Test()
        {
            var entity = new IdentityEntity();
            entity.Name = "oh hai";
            entity.Values.Add("timestamp", DateTime.Now);
            entity.Values.Add("hide", true);
            _identityTable1.Insert(entity);
            entity.Id.ShouldBeGreaterThan(0);
            var newEntity = _identityTable1.First(x => x.Id == entity.Id);
            newEntity.Name.ShouldEqual("oh hai");
            newEntity.Values["timestamp"].ToString().ShouldEqual(entity.Values["timestamp"].ToString());
            newEntity.Values["hide"].ShouldEqual(entity.Values["hide"]);
        }

        [Test]
        public void should_insert_entity_with_null_value()
        {
            var entity = new IdentityEntity();
            _identityTable1.Insert(entity);
            entity.Id.ShouldNotEqual(0);
            var newEntity = _identityTable1.First(x => x.Id == entity.Id);
            newEntity.Name.ShouldBeNull();
        }

        [Test]
        public void Copy_Into_Existing_Test()
        {
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", 
                _database.FirstTable.Name).ShouldEqual(10);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", 
                _database.ThirdTable.Name).ShouldEqual(5);
            _identityTable1.Take(7).CopyTo(_identityTable2).Count().ShouldEqual(12);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", 
                _database.FirstTable.Name).ShouldEqual(10);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0}", 
                _database.ThirdTable.Name).ShouldEqual(12);
        }

        [Test]
        public void Copy_Into_Narrowing_Test()
        {
            Assert.Throws<StringColumnNarrowingException>(() => _identityTable1.Take(7).CopyTo(_identityTable3));
        }

        [Test]
        public void should_sync_including_fields()
        {
            var importId = (object)Guid.NewGuid();
            _database.ExecuteNonQuery("UPDATE {0} SET name='ed', code=11, hide=1, upc=NULL WHERE id=1", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='don', code=22, hide=0, upc=NULL WHERE id=2", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='tomas', code=33, hide=1, upc='{1}' WHERE id=3", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='hector', code=44, hide=0, upc='{1}' WHERE id=4", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='eduardo', code=55, hide=1, upc='{1}' WHERE id=5", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='louie', code=66, hide=1, upc='{1}' WHERE id=6", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='ed', code=77, hide=0, upc='{1}' WHERE id=7", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='tomas', code=88, hide=0, upc='{1}' WHERE id=8", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='hector', code=99, hide=1, upc='{1}' WHERE id=9", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='eduardo', code=1010, hide=0, upc='{1}' WHERE id=10", _database.FirstTable.Name, importId);
            _identityTable1.Where(x => x.Values["upc"] != importId).SyncWith(_identityTable1.Where(x => x.Values["upc"] == importId), x => x.Name, SyncFields.Include, x => x.Values["hide"], x => x.Values["code"]);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='ed' AND code=77 AND hide=0 AND upc IS NULL AND id=1", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='don' AND code=22 AND hide=0 AND upc IS NULL AND id=2", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='tomas' AND code=88 AND hide=0 AND upc='{1}' AND id=3", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='hector' AND code=99 AND hide=1 AND upc='{1}' AND id=4", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='eduardo' AND code=1010 AND hide=0 AND upc='{1}' AND id=5", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='louie' AND code=66 AND hide=1 AND upc='{1}' AND id=6", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='ed' AND code=77 AND hide=0 AND upc='{1}' AND id=7", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='tomas' AND code=88 AND hide=0 AND upc='{1}' AND id=8", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='hector' AND code=99 AND hide=1 AND upc='{1}' AND id=9", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='eduardo' AND code=1010 AND hide=0 AND upc='{1}' AND id=10", _database.FirstTable.Name, importId).ShouldEqual(1);
        }

        [Test]
        public void should_sync_excluding_fields()
        {
            var importId = (object)Guid.NewGuid();
            _database.ExecuteNonQuery("UPDATE {0} SET name='ed', code=11, hide=1, upc=NULL WHERE id=1", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='don', code=22, hide=0, upc=NULL WHERE id=2", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='tomas', code=33, hide=1, upc='{1}' WHERE id=3", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='hector', code=44, hide=0, upc='{1}' WHERE id=4", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='eduardo', code=55, hide=1, upc='{1}' WHERE id=5", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='louie', code=66, hide=1, upc='{1}' WHERE id=6", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='ed', code=77, hide=0, upc='{1}' WHERE id=7", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='tomas', code=88, hide=0, upc='{1}' WHERE id=8", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='hector', code=99, hide=1, upc='{1}' WHERE id=9", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='eduardo', code=1010, hide=0, upc='{1}' WHERE id=10", _database.FirstTable.Name, importId);
            
            _identityTable1.Where(x => x.Values["upc"] != importId).SyncWith(_identityTable1.Where(x => x.Values["upc"] == importId), x => x.Name, SyncFields.Exclude, x => x.Id, x => x.Name, x => x.Values["upc"]);
            
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='ed' AND code=77 AND hide=0 AND upc IS NULL AND id=1", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='don' AND code=22 AND hide=0 AND upc IS NULL AND id=2", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='tomas' AND code=88 AND hide=0 AND upc='{1}' AND id=3", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='hector' AND code=99 AND hide=1 AND upc='{1}' AND id=4", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='eduardo' AND code=1010 AND hide=0 AND upc='{1}' AND id=5", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='louie' AND code=66 AND hide=1 AND upc='{1}' AND id=6", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='ed' AND code=77 AND hide=0 AND upc='{1}' AND id=7", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='tomas' AND code=88 AND hide=0 AND upc='{1}' AND id=8", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='hector' AND code=99 AND hide=1 AND upc='{1}' AND id=9", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='eduardo' AND code=1010 AND hide=0 AND upc='{1}' AND id=10", _database.FirstTable.Name, importId).ShouldEqual(1);
        }

        [Test]
        public void should_sync_excluding_fields_and_computed_columns()
        {
            var importId = (object)Guid.NewGuid();
            _database.ExecuteNonQuery("ALTER TABLE {0} ADD name_length AS LEN(name)", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='ed', code=11, hide=1, upc=NULL WHERE id=1", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='don', code=22, hide=0, upc=NULL WHERE id=2", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='tomas', code=33, hide=1, upc='{1}' WHERE id=3", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='hector', code=44, hide=0, upc='{1}' WHERE id=4", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='eduardo', code=55, hide=1, upc='{1}' WHERE id=5", _database.FirstTable.Name, Guid.Empty);
            _database.ExecuteNonQuery("UPDATE {0} SET name='louie', code=66, hide=1, upc='{1}' WHERE id=6", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='ed', code=77, hide=0, upc='{1}' WHERE id=7", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='tomas', code=88, hide=0, upc='{1}' WHERE id=8", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='hector', code=99, hide=1, upc='{1}' WHERE id=9", _database.FirstTable.Name, importId);
            _database.ExecuteNonQuery("UPDATE {0} SET name='eduardo', code=1010, hide=0, upc='{1}' WHERE id=10", _database.FirstTable.Name, importId);

            _identityTable1.Where(x => x.Values["upc"] != importId).SyncWith(_identityTable1.Where(x => x.Values["upc"] == importId), x => x.Name, SyncFields.Exclude, x => x.Id, x => x.Name, x => x.Values["upc"]);

            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='ed' AND code=77 AND hide=0 AND upc IS NULL AND id=1 AND name_length = 2", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='don' AND code=22 AND hide=0 AND upc IS NULL AND id=2 AND name_length = 3", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='tomas' AND code=88 AND hide=0 AND upc='{1}' AND id=3 AND name_length = 5", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='hector' AND code=99 AND hide=1 AND upc='{1}' AND id=4 AND name_length = 6", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='eduardo' AND code=1010 AND hide=0 AND upc='{1}' AND id=5 AND name_length = 7", _database.FirstTable.Name, Guid.Empty).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='louie' AND code=66 AND hide=1 AND upc='{1}' AND id=6 AND name_length = 5", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='ed' AND code=77 AND hide=0 AND upc='{1}' AND id=7 AND name_length = 2", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='tomas' AND code=88 AND hide=0 AND upc='{1}' AND id=8 AND name_length = 5", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='hector' AND code=99 AND hide=1 AND upc='{1}' AND id=9 AND name_length = 6", _database.FirstTable.Name, importId).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='eduardo' AND code=1010 AND hide=0 AND upc='{1}' AND id=10 AND name_length = 7", _database.FirstTable.Name, importId).ShouldEqual(1);
        }

        [Test]
        public void should_sync_excluding_fields_from_different_table()
        {
            _identityTable1.CopyTo(_identityTable2);
            _database.ExecuteNonQuery("UPDATE {0} SET name='ed', code=11, hide=1 WHERE id=1", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='don', code=22, hide=0 WHERE id=2", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='tomas', code=33, hide=1 WHERE id=3", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='hector', code=44, hide=0 WHERE id=4", _database.FirstTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='eduardo', code=55, hide=1 WHERE id=5", _database.FirstTable.Name);
            
            _database.ExecuteNonQuery("UPDATE {0} SET name='louie', code=66, hide=1 WHERE id=6", _database.ThirdTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='ed', code=77, hide=0 WHERE id=7", _database.ThirdTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='tomas', code=88, hide=0 WHERE id=8", _database.ThirdTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='hector', code=99, hide=1 WHERE id=9", _database.ThirdTable.Name);
            _database.ExecuteNonQuery("UPDATE {0} SET name='eduardo', code=1010, hide=0 WHERE id=10", _database.ThirdTable.Name);
            
            _identityTable1.SyncWith(_identityTable2, x => x.Name, SyncFields.Exclude, x => x.Id, x => x.Name, x => x.Values["upc"]);
            
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='ed' AND code=77 AND hide=0 AND id=1", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='don' AND code=22 AND hide=0 AND id=2", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='tomas' AND code=88 AND hide=0 AND id=3", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='hector' AND code=99 AND hide=1 AND id=4", _database.FirstTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='eduardo' AND code=1010 AND hide=0 AND id=5", _database.FirstTable.Name).ShouldEqual(1);

            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='louie' AND code=66 AND hide=1 AND id=6", _database.ThirdTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='ed' AND code=77 AND hide=0 AND id=7", _database.ThirdTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='tomas' AND code=88 AND hide=0 AND id=8", _database.ThirdTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='hector' AND code=99 AND hide=1 AND id=9", _database.ThirdTable.Name).ShouldEqual(1);
            _database.ExecuteScalar<int>("SELECT COUNT(*) FROM {0} WHERE name='eduardo' AND code=1010 AND hide=0 AND id=10", _database.ThirdTable.Name).ShouldEqual(1);
        }
    }
}
