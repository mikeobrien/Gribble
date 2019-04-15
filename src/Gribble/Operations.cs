using System;
using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;
using Gribble.Model;
using Gribble.TransactSql;

namespace Gribble
{
    public class Operations
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IEntityMapping _mapping;
        private readonly IProfiler _profiler;
        private readonly bool _noLock;

        public Operations(IConnectionManager connectionManager, 
            IEntityMapping mapping, IProfiler profiler, bool noLock)
        {
            _connectionManager = connectionManager;
            _mapping = mapping;
            _profiler = profiler;
            _noLock = noLock;
        }

        public TResult ExecuteQuery<TEntity, TResult>(Select select)
        {
            IEnumerable<string> columns = null;
            if (select.From.HasQueries)
            {
                var columnsStatement = SchemaWriter.CreateUnionColumnsStatement(select);
                columns = Command.Create(columnsStatement, _profiler)
                    .ExecuteEnumerable<string>(_connectionManager);
            }
            var selectStatement = SelectWriter<TEntity>.CreateStatement(
                select, _mapping, columns, _noLock);
            return (TResult)(new Loader<TEntity>(Command.Create(selectStatement, 
                _profiler), _mapping).Execute(_connectionManager));
        }

        public IQueryable<TEntity> CopyInto<TEntity>(Insert insert)
        {
            insert.Query.Projection = GetSharedColumns(insert.Query, insert.Into);
            var statement = InsertWriter<TEntity>.CreateStatement(insert, _mapping);
            Command.Create(statement, _profiler).ExecuteNonQuery(_connectionManager);
            return new Table<TEntity>(_connectionManager, insert.Into.Name, _mapping, _profiler, _noLock);
        }

        public IQueryable<TEntity> SyncWith<TEntity>(Sync sync)
        {
            if (!sync.Target.HasProjection)
            {
                var fields = GetSharedColumns(sync.Source, sync.Target.From.Table);
                Func<string, IList<SelectProjection>> createProjection = 
                    alias => fields.Select(x => new SelectProjection { 
                        Projection = new Projection { Type = Projection.ProjectionType.Field, 
                            Field = new Field {
                                Name = x.Projection.Field.Name,
                                Key = x.Projection.Field.Key,
                                HasKey = x.Projection.Field.HasKey,
                                TableAlias = alias }}}).ToList();
                sync.Target.Projection = createProjection(sync.Target.From.Alias);
                sync.Source.Projection = createProjection(sync.Source.From.Alias);
            }
            
            var statement = SyncWriter<TEntity>.CreateStatement(sync, _mapping);
            Command.Create(statement, _profiler).ExecuteNonQuery(_connectionManager);
            return new Table<TEntity>(_connectionManager, sync.Target
                .From.Table.Name, _mapping, _profiler, _noLock);
        }

        public IList<SelectProjection> GetSharedColumns(Select source, Table target)
        {
            var hasIdentityKey = _mapping.Key.KeyType == PrimaryKeyType.Integer && 
                                 _mapping.Key.KeyGeneration == PrimaryKeyGeneration.Server;
            var keyColumnName = _mapping.Key.GetColumnName();
            var columns = Command.Create(SchemaWriter
                    .CreateSharedColumnsStatement(source, target), _profiler)
                .ExecuteEnumerable(_connectionManager, r => new
                {
                    Column = TableSchema.ColumnFactory(r),
                    IsNarrowing = (bool)r[SqlWriter.Aliases.IsNarrowing]
                })
                .Where(x => (!hasIdentityKey || !x.Column.Name.Equals(keyColumnName, 
                                 StringComparison.OrdinalIgnoreCase)) && !x.Column.IsComputed);
            if (columns.Any(x => x.IsNarrowing))
                throw new StringColumnNarrowingException(columns.Select(x => x.Column.Name));
            return columns.Select(x => x.Column).Select(x => new SelectProjection {
                Projection = Projection.Create.Field(_mapping.Column.GetPropertyName(x.Name), 
                    !_mapping.Column.HasStaticPropertyMapping(x.Name))}).ToList();
        } 
    }
}