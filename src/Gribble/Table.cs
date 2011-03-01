using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Gribble.Expressions;
using Gribble.Mapping;
using Gribble.Statements;
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
        private readonly ConnectionManager _connectionManager;
        private readonly string _table;
        private readonly IEntityMapping _map;
        private readonly IProfiler _profiler;

        public Table(SqlConnection connection, TimeSpan commandTimeout, string table, IEntityMapping mapping) :
            this(connection, commandTimeout, table, mapping, null) { }

        public Table(SqlConnection connection, TimeSpan commandTimeout, string table, IEntityMapping mapping, bool profile) : 
            this(connection, commandTimeout, table, mapping, profile ? new ConsoleProfiler() : null) { }

        public Table(SqlConnection connection, TimeSpan commandTimeout, string table, IEntityMapping mapping, IProfiler profiler) :
            this(new ConnectionManager(connection, commandTimeout), table, mapping, profiler) { }

        public Table(ConnectionManager connectionManagerManager, string table, IEntityMapping mapping) :
            this(connectionManagerManager, table, mapping, null) { }

        public Table(ConnectionManager connectionManagerManager, string table, IEntityMapping mapping, bool profile) :
            this(connectionManagerManager, table, mapping, profile ? new ConsoleProfiler() : null) { }

        public Table(ConnectionManager connectionManagerManager, string table, IEntityMapping mapping, IProfiler profiler)
        {
            _connectionManager = connectionManagerManager;
            _table = table;
            _map = mapping;
            _profiler = profiler;
        }

        public string Name { get { return _table; } }

        public override QueryableBase<TEntity> CreateQuery()
        {
            return new Table<TEntity>(_connectionManager, _table, _map, _profiler);
        }

        public override TResult ExecuteQuery<TResult>(Expression expression)
        {
            var select = SelectVisitor<TEntity>.CreateModel(expression, x => ((Table<TEntity>) x).Name);
            return select.Target.Type == Data.DataType.Query ? 
                ExecuteQuery<TResult>(select) : 
                (TResult)CopyInto(select);
        }

        public void Insert(TEntity entity)
        {
            var adapter = new EntityAdapter<TEntity>(entity, _map);
            var hasIdentityKey = _map.Key.KeyType == PrimaryKeyType.IdentitySeed;
            var keyColumnName = _map.Key.GetColumnName();
            if (_map.Key.KeyType == PrimaryKeyType.GuidClientGenerated) adapter.Key = _map.Key.GenerateGuidKey();
            var values = adapter.GetValues().Where(x => !hasIdentityKey || (x.Key != keyColumnName));
            var command = Command.Create(InsertWriter<TEntity>.CreateStatement(new Insert(values, hasIdentityKey, _table), _map), _profiler);

            if (command.Statement.Result == Statement.ResultType.None) command.ExecuteNonQuery(_connectionManager);
            else adapter.Key = command.ExecuteScalar(_connectionManager);
        }

        public void Update(TEntity entity)
        {
            var adapter = new EntityAdapter<TEntity>(entity, _map);
            var keyColumnName = _map.Key.GetColumnName();
            var values = adapter.GetValues().Where(x => x.Key != keyColumnName);
            Command.Create(UpdateWriter<TEntity>.CreateStatement(new Update(values, _table, GetKeyFilter(entity, adapter)), _map), _profiler).
                    ExecuteNonQuery(_connectionManager);
        }

        public void Delete(TEntity entity)
        {
            Delete(GetKeyFilter(entity), false);
        }

        public void Delete(Expression<Func<TEntity, bool>> filter)
        {
            Delete(WhereVisitor<TEntity>.CreateModel(filter), false);
        }

        public void DeleteMany(Expression<Func<TEntity, bool>> filter) 
        {
            Delete(WhereVisitor<TEntity>.CreateModel(filter), true); 
        }

        private void Delete(Operator filter, bool multiDelete)
        {
            Command.Create(DeleteWriter<TEntity>.CreateStatement(new Delete(_table, filter, multiDelete), _map), _profiler).
                    ExecuteNonQuery(_connectionManager);
        }

        private Operator GetKeyFilter(TEntity entity, EntityAdapter<TEntity> adapter = null)
        {
            var id = (adapter ?? new EntityAdapter<TEntity>(entity, _map)).Key;
            var field = _map.Key.GetPropertyName();
            return Operator.Create.FieldEqualsConstant(field, id);
        }

        private TResult ExecuteQuery<TResult>(Select select)
        {
            IEnumerable<string> columns = null;
            if (select.Source.HasQueries)
            {
                var columnsStatement = SchemaWriter.CreateUnionColumnsStatement(select);
                columns = Command.Create(columnsStatement, _profiler).ExecuteEnumerable<string>(_connectionManager);
            }
            var selectStatement = SelectWriter<TEntity>.CreateStatement(select, _map, columns);
            return (TResult)(new Loader<TEntity>(Command.Create(selectStatement, _profiler), _map).Execute(_connectionManager));
        }

        private IQueryable<TEntity> CopyInto(Select select)
        {
            var hasIdentityKey = _map.Key.KeyType == PrimaryKeyType.IdentitySeed;
            var keyColumnName = _map.Key.GetColumnName();

            var columns = Command.Create(SchemaWriter.CreateTableExistsStatement(select.Target.Table.Name), _profiler).
                                      ExecuteScalar<bool>(_connectionManager) ?
                              GetColumnNames(select, hasIdentityKey, keyColumnName) :
                              CreateTable(select, hasIdentityKey, keyColumnName);
            select.Projection = columns.Select(x => new SelectProjection
            {
                Projection = Projection.Create.Field(_map.Column.GetPropertyName(x),
                                                     !_map.Column.HasStaticPropertyMapping(x))
            }).ToList();
            var statement = InsertWriter<TEntity>.CreateStatement(new Insert(select, columns, select.Target.Table.Name), _map);
            Command.Create(statement, _profiler).ExecuteNonQuery(_connectionManager);

            return new Table<TEntity>(_connectionManager, select.Target.Table.Name, _map, _profiler);
        }

        private IEnumerable<string> CreateTable(Select select, bool hasIdentityKey, string keyColumnName)
        {
            var columns = Command.Create(SchemaWriter.CreateCreateTableColumnsStatement(select), _profiler).
                ExecuteEnumerable<string, byte, short, bool, bool, bool, object, bool>(_connectionManager).
                Select(x => new Column
                {
                    Name = x.Item1,
                    Type = DataTypes.GetClrType(x.Item2),
                    Length = x.Item3,
                    IsNullable = x.Item4,
                    IsIdentity = x.Item5,
                    IsAutoGenerated = x.Item6,
                    DefaultValue = x.Item7 != DBNull.Value ? Convert.ChangeType(x.Item7, DataTypes.GetClrType(x.Item2)) : null,
                    IsPrimaryKey = x.Rest.Item1
                });
            Command.Create(SchemaWriter.CreateTableCreateStatement(select.Target.Table.Name, columns.ToArray()), _profiler).
                    ExecuteNonQuery(_connectionManager);
            return columns.Where(x => !hasIdentityKey || !x.Name.Equals(keyColumnName, StringComparison.OrdinalIgnoreCase)).
                           Select(x => x.Name);
        }

        private IEnumerable<string> GetColumnNames(Select select, bool hasIdentityKey, string keyColumnName)
        {
            var columns = Command.Create(SchemaWriter.CreateSelectIntoColumnsStatement(select), _profiler).
                                  ExecuteEnumerable<string, bool>(_connectionManager).
                                  Where(x => !hasIdentityKey || !x.Item1.Equals(keyColumnName, StringComparison.OrdinalIgnoreCase));
            if (columns.Any(x => x.Item2)) throw new StringColumnNarrowingException(columns.Select(x => x.Item1));
            select.Projection = columns.Select(x => new SelectProjection
            {
                Projection = Projection.Create.Field(_map.Column.GetPropertyName(x.Item1),
                                                     !_map.Column.HasStaticPropertyMapping(x.Item1))
            }).ToList();
            return columns.Select(x => x.Item1);
        }
    }
}
