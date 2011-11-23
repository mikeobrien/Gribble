using System;
using System.Data;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Conventions.Helpers;
using Gribble;
using Gribble.Mapping;
using Gribble.Model;
using NUnit.Framework;
using ConnectionManager = Gribble.NHibernate.ConnectionManager;

namespace Tests.NHibernate
{
    [TestFixture]
    public class ConnectionManagerTests
    {
        public static string TableName = TestTable.GenerateName();

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

        [Test]
        public void Gribble_Should_Share_Transaction_With_Nhibernate()
        {
            var testDatabase = new TestDatabase();
            testDatabase.SetUp();
            var sessionFactory = Fluently.Configure().
                    Sql2008Database(testDatabase.Connection.ConnectionString, IsolationLevel.ReadCommitted, true).
                    Mappings(map => map.FluentMappings.Add<EntityMap>().Conventions.Add(AutoImport.Never())).
                    ExposeConfiguration(config => config.AutoQuote()).
                    BuildConfiguration();

            using (var factory = sessionFactory.BuildSessionFactory())
            using (var session = factory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var connectionManager = new ConnectionManager(session, TimeSpan.FromMinutes(1));
                var database = new Database(connectionManager, new EntityMappingCollection(Enumerable.Empty<IClassMap>()));
                database.CreateTable(TableName, 
                    new Column { Name = "id", IsPrimaryKey = true, IsIdentity = true, Type = typeof(int) },
                    new Column { Name = "name", Type = typeof(string), Length = 500 });

                var entity = new Entity {Name = "Dirac"};
                session.Save(entity);
                transaction.Commit();
            }
        }
    }
}
