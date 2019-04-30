﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gribble.Expressions;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.Model;
using Gribble.TransactSql;
using NUnit.Framework;
using Should;

namespace Tests.TransactSql
{
    [TestFixture]
    public class SyncWriterTests
    {
        public class Entity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Flag { get; set; }
            public DateTime? Created { get; set; }
            public Guid ImportId { get; set; }
            public IDictionary<string, object> Values { get; set; }
        }

        public class EntityMap : ClassMap<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Flag).Column("flag");
                Map(x => x.Created).Column("created");
                Id(x => x.ImportId).Column("import_id");
                Map(x => x.Values).Dynamic();
            }
        }

        private static readonly EntityMapping Map = new EntityMapping(new EntityMap());

        public const string TableName1 = "XLIST_1";
        public const string TableName2 = "XLIST_2";

        private IEntityMapping _mapping;

        [SetUp]
        public void Setup()
        {
            _mapping = new EntityMapping(new AutoClassMap<Entity>());
        }

        [Test]
        public void should_render_sync_include_sql()
        {
            var target = MockQueryable<Entity>.Create(TableName1);
            var source = MockQueryable<Entity>.Create(TableName2);
            var importId = Guid.NewGuid();
            target.Where(x => x.ImportId != importId).SyncWith(source.Where(x => x.ImportId == importId), x => x.Created, SyncFields.Include, x => x.Name, x => x.Flag);
            var query = QueryVisitor<Entity>.CreateModel(target.Expression, x => ((MockQueryable<Entity>)x).Name, _mapping);

            query.Operation.ShouldEqual(Query.OperationType.SyncWith);
            query.SyncWith.ShouldNotBeNull();

            var sync = query.SyncWith;

            var statement = SyncWriter<Entity>.CreateStatement(sync, Map);

            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(importId);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(importId);
            statement.Text.ShouldEqual(string.Format(
                "UPDATE [{0}] SET [{0}].[name] = [{1}].[name], [{0}].[flag] = [{1}].[flag] FROM [{2}] [{0}] INNER JOIN [{3}] [{1}] ON [{0}].[created] = [{1}].[created] AND " +
                "([{1}].[import_id] = @{4}) WHERE (([{0}].[import_id] <> @{5}) OR [{0}].[import_id] IS NULL)",
                sync.Target.From.Alias, sync.Source.From.Alias, TableName1, TableName2,
                statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void should_render_sync_exclude_sql()
        {
            var target = MockQueryable<Entity>.Create(TableName1);
            var source = MockQueryable<Entity>.Create(TableName2);
            var importId = Guid.NewGuid();
            target.Where(x => x.ImportId != importId).SyncWith(source.Where(x => x.ImportId == importId), x => x.Created, SyncFields.Exclude, x => x.Id, x => x.Created);
            var query = QueryVisitor<Entity>.CreateModel(target.Expression, x => ((MockQueryable<Entity>)x).Name, _mapping);

            query.Operation.ShouldEqual(Query.OperationType.SyncWith);
            query.SyncWith.ShouldNotBeNull();

            var sync = query.SyncWith;

            sync.Target.Projection.ShouldBeNull();
            sync.Source.Projection.ShouldBeNull();

            sync.Target.Projection = new List<SelectProjection> 
                { new SelectProjection { Projection = new Projection { Type = Projection.ProjectionType.Field, Field = new Field { Name = "Id", TableAlias = sync.Target.From.Alias }}},
                  new SelectProjection { Projection = new Projection { Type = Projection.ProjectionType.Field, Field = new Field { Name = "Created", TableAlias = sync.Target.From.Alias }}},
                  new SelectProjection { Projection = new Projection { Type = Projection.ProjectionType.Field, Field = new Field { Name = "Name", TableAlias = sync.Target.From.Alias }}},
                  new SelectProjection { Projection = new Projection { Type = Projection.ProjectionType.Field, Field = new Field { Name = "Flag", TableAlias = sync.Target.From.Alias }}}};

            sync.Source.Projection = new List<SelectProjection> 
                { new SelectProjection { Projection = new Projection { Type = Projection.ProjectionType.Field, Field = new Field { Name = "Id", TableAlias = sync.Source.From.Alias }}},
                  new SelectProjection { Projection = new Projection { Type = Projection.ProjectionType.Field, Field = new Field { Name = "Created", TableAlias = sync.Source.From.Alias }}},
                  new SelectProjection { Projection = new Projection { Type = Projection.ProjectionType.Field, Field = new Field { Name = "Name", TableAlias = sync.Source.From.Alias }}},
                  new SelectProjection { Projection = new Projection { Type = Projection.ProjectionType.Field, Field = new Field { Name = "Flag", TableAlias = sync.Source.From.Alias }}}};

            var statement = SyncWriter<Entity>.CreateStatement(sync, Map);

            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(importId);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(importId);
            statement.Text.ShouldEqual(string.Format(
                "UPDATE [{0}] SET [{0}].[name] = [{1}].[name], [{0}].[flag] = [{1}].[flag] FROM [{2}] [{0}] INNER JOIN [{3}] [{1}] ON [{0}].[created] = [{1}].[created] AND " +
                "([{1}].[import_id] = @{4}) WHERE (([{0}].[import_id] <> @{5}) OR [{0}].[import_id] IS NULL)",
                sync.Target.From.Alias, sync.Source.From.Alias, TableName1, TableName2,
                statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void should_render_dynamic_value_sql()
        {
            var target = MockQueryable<Entity>.Create(TableName1);
            var source = MockQueryable<Entity>.Create(TableName2);
            var importId = (object)Guid.NewGuid();
            target.Where(x => x.Values["uid"] != importId).SyncWith(source.Where(x => x.Values["uid"] == importId), x => x.Created, SyncFields.Include, x => x.Name, x => x.Values["yada"]);
            var query = QueryVisitor<Entity>.CreateModel(target.Expression, x => ((MockQueryable<Entity>)x).Name, _mapping);

            query.Operation.ShouldEqual(Query.OperationType.SyncWith);
            query.SyncWith.ShouldNotBeNull();

            var sync = query.SyncWith;

            var statement = SyncWriter<Entity>.CreateStatement(sync, Map);

            statement.Result.ShouldEqual(Statement.ResultType.None);
            statement.Parameters.Count.ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(importId);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(importId);
            statement.Text.ShouldEqual(string.Format(
                "UPDATE [{0}] SET [{0}].[name] = [{1}].[name], [{0}].[yada] = [{1}].[yada] FROM [{2}] [{0}] INNER JOIN [{3}] [{1}] ON [{0}].[created] = [{1}].[created] AND " +
                "([{1}].[uid] = @{4}) WHERE (([{0}].[uid] <> @{5}) OR [{0}].[uid] IS NULL)",
                sync.Target.From.Alias, sync.Source.From.Alias, TableName1, TableName2,
                statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }
    }
}
