using System;
using System.Data;
using FluentNHibernate.Cfg;
using FluentNHibernate.Conventions.Helpers;
using Gribble;
using Gribble.Model;
using NHibernate.Cfg;
using NUnit.Framework;
using Should;
using ConnectionManager = Gribble.NHibernate.ConnectionManager;

namespace Tests.NHibernate
{
    [TestFixture]
    public class ConnectionManagerTests
    {
        private readonly static string TableName = TestTable.GenerateName();
        private TestDatabase _database;
        private const int CommandTimeout = 1200;

        private readonly static Func<TestDatabase, Configuration> CreateConfiguration = database => Fluently.Configure().
                Sql2008Database(database.Connection.ConnectionString, IsolationLevel.ReadCommitted, true).
                Mappings(map => map.FluentMappings.Add<EntityMap>().Conventions.Add(AutoImport.Never())).
                ExposeConfiguration(config => config.AutoQuote().CommandTimeout(CommandTimeout)).
                BuildConfiguration();

        public class Entity
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }

        public class EntityMap : FluentNHibernate.Mapping.ClassMap<Entity>
        {
            public EntityMap()
            {
                Table(TableName);

                DynamicUpdate();
                DynamicInsert();

                LazyLoad();

                Id(x => x.Id).Column("id").GeneratedBy.Identity();
                Map(x => x.Name).Column("name");
            }
        }

        [SetUp]
        public void Setup()
        {
            _database = new TestDatabase();
            _database.SetUp();
        }

        [TearDown]
        public void TearDown() { _database.TearDown(); }

        [Test]
        public void Gribble_Commands_Should_Pick_Up_NHibernate_Command_Timeout()
        {
            using (var factory = CreateConfiguration(_database).BuildSessionFactory())
            using (var session = factory.OpenSession())
            {
                var connectionManager = new ConnectionManager(session);
                connectionManager.CreateCommand().CommandTimeout.ShouldEqual(CommandTimeout);
            }
        }

        [Test]
        public void Gribble_Should_Share_Transaction_With_Nhibernate()
        {
            using (var factory = CreateConfiguration(_database).BuildSessionFactory())
            using (var session = factory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var connectionManager = new ConnectionManager(session);
                var database = Database.Create(connectionManager);
                database.CreateTable(TableName,
                    new Column("id", typeof(int), key: Column.KeyType.PrimaryKey, isIdentity: true),
                    new Column("name", typeof(string), length: 500));

                var entity = new Entity {Name = "Dirac"};
                session.Save(entity);
                transaction.Commit();
            }
        }

        [Test]
        public void Gribble_Should_Work_Without_A_Transaction_With_Nhibernate()
        {
            using (var factory = CreateConfiguration(_database).BuildSessionFactory())
            using (var session = factory.OpenSession())
            {
                var connectionManager = new ConnectionManager(session);
                var database = Database.Create(connectionManager);
                database.CreateTable(TableName,
                    new Column("id", typeof(int), key: Column.KeyType.PrimaryKey, isIdentity: true),
                    new Column("name", typeof(string), length: 500));

                var entity = new Entity { Name = "Dirac" };
                session.Save(entity);
            }
        }
    }
}
