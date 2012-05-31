using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Gribble.Expressions;
using Gribble.Mapping;
using Gribble.Model;
using Gribble.TransactSql;

namespace Gribble
{
    public class StringColumnNarrowingException : Exception
    {
        public StringColumnNarrowingException(IEnumerable<string> columns) :
            base(string.Format("Unable to convert the following columns as a narrowing conversion would occur: {0}", columns.Aggregate((i,x) => i + ", " + x))) { }
    }

    public class Table<TEntity> : QueryableBase<TEntity>, ITable<TEntity>
    {
        private readonly IConnectionManager _connectionManager;
        private readonly string _table;
        private readonly IEntityMapping _map;
        private readonly IProfiler _profiler;
        private readonly bool _noLock;

        public Table(IConnectionManager connectionManagerManager, string table, IEntityMapping mapping, IProfiler profiler, bool noLock)
        {
            _connectionManager = connectionManagerManager;
            _table = table;
            _map = mapping;
            _profiler = profiler;
            _noLock = noLock;
        }

        public static ITable<TEntity> Create<TKey>(SqlConnection connection, string tableName, string keyColumn, TimeSpan? commandTimeout = null, IProfiler profiler = null, bool noLock = false)
        {
            return Create<TKey>(new ConnectionManager(connection, commandTimeout ?? new TimeSpan(0, 5, 0)), keyColumn, tableName, profiler ?? new ConsoleProfiler(), noLock);
        }

        public static ITable<TEntity> Create<TKey>(IConnectionManager connectionManager, string tableName, string keyColumn, IProfiler profiler = null, bool noLock = false)
        {
            var mapping = new EntityMapping(typeof(Guid) == typeof(TKey) ? new GuidKeyEntityMap(keyColumn) : (typeof(int) == typeof(TKey) ? (IClassMap)new IntKeyEntityMap(keyColumn) : null));
            return new Table<TEntity>(connectionManager, tableName, mapping, profiler ?? new ConsoleProfiler(), noLock);
        }

        public static ITable<TEntity> Create(SqlConnection connection, string tableName, IEntityMapping entityMapping, TimeSpan? commandTimeout = null, IProfiler profiler = null, bool noLock = false)
        {
            return Create(new ConnectionManager(connection, commandTimeout ?? new TimeSpan(0, 5, 0)), tableName, entityMapping, profiler ?? new ConsoleProfiler(), noLock);
        }

        public static ITable<TEntity> Create(IConnectionManager connectionManager, string tableName, IEntityMapping entityMapping, IProfiler profiler = null, bool noLock = false)
        {
            return new Table<TEntity>(connectionManager, tableName, entityMapping, profiler ?? new ConsoleProfiler(), noLock);
        }

        public string Name { get { return _table; } }

        public override QueryableBase<TEntity> CreateQuery()
        {
            return new Table<TEntity>(_connectionManager, _table, _map, _profiler, _noLock);
        }

        public override TResult ExecuteQuery<TResult>(Expression expression)
        {
            var query = QueryVisitor<TEntity>.CreateModel(expression, x => ((Table<TEntity>) x).Name);
            switch (query.Operation) {
                case Query.OperationType.Query: return ExecuteQuery<TResult>(query.Select);
                case Query.OperationType.CopyTo: return (TResult)CopyInto(query.CopyTo);
                case Query.OperationType.SyncWith: return (TResult)SyncWith(query.SyncWith);
                default: throw new NotImplementedException();
            }
        }

        public void Insert(TEntity entity)
        {
            var adapter = new EntityAdapter<TEntity>(entity, _map);
            var hasIdentityKey = _map.Key.KeyType == PrimaryKeyType.IdentitySeed;
            var keyColumnName = _map.Key.GetColumnName();
            if (_map.Key.KeyType == PrimaryKeyType.GuidClientGenerated) adapter.Key = _map.Key.GenerateGuidKey();
            var values = adapter.GetValues().Where(x => !hasIdentityKey || (x.Key != keyColumnName)).ToDictionary(x => x.Key, x => x.Value);
            var insert = new Insert { HasIdentityKey = hasIdentityKey, Type = Model.Insert.SetType.Values, Into = new Table {Name = _table}, Values = values};
            var command = Command.Create(InsertWriter<TEntity>.CreateStatement(insert, _map), _profiler);

            if (command.Statement.Result == Statement.ResultType.None) command.ExecuteNonQuery(_connectionManager);
            else adapter.Key = command.ExecuteScalar(_connectionManager);
        }

        public TEntity Get<T>(T id)
        {
            var select = new Select {
                Top = 1,
                From = { Type = Data.DataType.Table, Table = new Table { Name = _table } },
                Where = CreateKeyFilter(id) };
            return ExecuteQuery<IEnumerable<TEntity>>(select).FirstOrDefault();
        }

        public void Update(TEntity entity)
        {
            var adapter = new EntityAdapter<TEntity>(entity, _map);
            var keyColumnName = _map.Key.GetColumnName();
            var values = adapter.GetValues().Where(x => x.Key != keyColumnName);
            Command.Create(UpdateWriter<TEntity>.CreateStatement(new Update(values, _table, CreateEntityKeyFilter(entity, adapter)), _map), _profiler).
                    ExecuteNonQuery(_connectionManager);
        }

        public void Delete<T>(T id)
        {
            Delete(CreateKeyFilter(id), false);
        }

        public void Delete(TEntity entity)
        {
            Delete(CreateEntityKeyFilter(entity), false);
        }

        public void Delete(Expression<Func<TEntity, bool>> filter)
        {
            Delete(WhereVisitor<TEntity>.CreateModel(filter), false);
        }

        public int DeleteMany(Expression<Func<TEntity, bool>> filter) 
        {
            return Delete(WhereVisitor<TEntity>.CreateModel(filter), true); 
        }

        public int DeleteMany(IQueryable<TEntity> source)
        {
            var query = QueryVisitor<TEntity>.CreateModel(source.Expression, x => ((Table<TEntity>) x).Name);
            return Command.Create(DeleteWriter<TEntity>.CreateStatement(
                new Delete(_table, query.Select, true), _map), _profiler).
                    ExecuteNonQuery(_connectionManager);
        }

        private int Delete(Operator filter, bool multiDelete)
        {
            return Command.Create(DeleteWriter<TEntity>.CreateStatement(
                new Delete(_table, filter, multiDelete), _map), _profiler).
                    ExecuteNonQuery(_connectionManager);
        }

        private Operator CreateKeyFilter<T>(T id)
        {
            var field = _map.Key.GetPropertyName();
            return Operator.Create.FieldEqualsConstant(field, id);
        }

        private Operator CreateEntityKeyFilter(TEntity entity, EntityAdapter<TEntity> adapter = null)
        {
            var id = (adapter ?? new EntityAdapter<TEntity>(entity, _map)).Key;
            var field = _map.Key.GetPropertyName();
            return Operator.Create.FieldEqualsConstant(field, id);
        }

        private TResult ExecuteQuery<TResult>(Select select)
        {
            IEnumerable<string> columns = null;
            if (select.From.HasQueries)
            {
                var columnsStatement = SchemaWriter.CreateUnionColumnsStatement(select);
                columns = Command.Create(columnsStatement, _profiler).ExecuteEnumerable<string>(_connectionManager);
            }
            var selectStatement = SelectWriter<TEntity>.CreateStatement(select, _map, columns, _noLock);
            return (TResult)(new Loader<TEntity>(Command.Create(selectStatement, _profiler), _map).Execute(_connectionManager));
        }

        private IQueryable<TEntity> CopyInto(Insert insert)
        {
            insert.Query.Projection = GetSharedColumns(insert.Query, insert.Into);
            var statement = InsertWriter<TEntity>.CreateStatement(insert, _map);
            Command.Create(statement, _profiler).ExecuteNonQuery(_connectionManager);
            return new Table<TEntity>(_connectionManager, insert.Into.Name, _map, _profiler, _noLock);
        }

        private IList<SelectProjection> GetSharedColumns(Select source, Table target)
        {
            var hasIdentityKey = _map.Key.KeyType == PrimaryKeyType.IdentitySeed;
            var keyColumnName = _map.Key.GetColumnName();
            var columns = Command.Create(SchemaWriter.CreateSharedColumnsStatement(source, target), _profiler).
                                  ExecuteEnumerable<string, bool>(_connectionManager).
                                  Where(x => !hasIdentityKey || !x.Item1.Equals(keyColumnName, StringComparison.OrdinalIgnoreCase));
            if (columns.Any(x => x.Item2)) throw new StringColumnNarrowingException(columns.Select(x => x.Item1));
            return columns.Select(x => x.Item1).Select(x => new SelectProjection {
                Projection = Projection.Create.Field(_map.Column.GetPropertyName(x), !_map.Column.HasStaticPropertyMapping(x))}).ToList();
        } 

        private IQueryable<TEntity> SyncWith(Sync sync)
        {
            if (!sync.Target.HasProjection)
            {
                var fields = GetSharedColumns(sync.Source, sync.Target.From.Table);
                Func<string, IList<SelectProjection>> createProjection = alias => fields.Select(x => new SelectProjection { 
                    Projection = new Projection { Type = Projection.ProjectionType.Field, 
                        Field = new Field { Name = x.Projection.Field.Name,
                                            Key = x.Projection.Field.Key,
                                            HasKey = x.Projection.Field.HasKey,
                                            TableAlias = alias }}}).ToList();
                sync.Target.Projection = createProjection(sync.Target.From.Alias);
                sync.Source.Projection = createProjection(sync.Source.From.Alias);
            }
            
            var statement = SyncWriter<TEntity>.CreateStatement(sync, _map);
            Command.Create(statement, _profiler).ExecuteNonQuery(_connectionManager);
            return new Table<TEntity>(_connectionManager, sync.Target.From.Table.Name, _map, _profiler, _noLock);
        }
    }
}
