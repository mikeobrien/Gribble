using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Gribble.Expressions;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.Model;
using Gribble.TransactSql;

namespace Gribble
{
    public interface ITable<TEntity> : IOrderedQueryable<TEntity>, INamedQueryable
    {
        TEntity Get<T>(T id);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete<T>(T id);
        void Delete(TEntity entity);
        void Delete(Expression<Func<TEntity, bool>> filter);
        int DeleteMany(Expression<Func<TEntity, bool>> filter);
        int DeleteMany(IQueryable<TEntity> source);
    }

    public class StringColumnNarrowingException : Exception
    {
        public StringColumnNarrowingException(IEnumerable<string> columns) :
            base("Unable to convert the following columns as a narrowing conversion " +
                 $"would occur: {columns.Aggregate((i, x) => i + ", " + x)}") { }
    }

    public class Table<TEntity> : ITable<TEntity> 
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IEntityMapping _mapping;
        private readonly IProfiler _profiler;
        private readonly Queryable<TEntity> _queryable;
        private readonly Operations _operations;

        public Table(IConnectionManager connectionManager, string tableName, 
            IEntityMapping mapping, IProfiler profiler, bool noLock = false)
        {
            _connectionManager = connectionManager;
            Name = tableName;
            _mapping = mapping;
            _profiler = profiler;
            _operations = new Operations(connectionManager, mapping, profiler, noLock);
            _queryable = new Queryable<TEntity>(Name, mapping, _operations);
        }

        public static ITable<TEntity> Create(
            SqlConnection connection, 
            string tableName, 
            string keyColumn, 
            TimeSpan? commandTimeout = null, 
            IProfiler profiler = null, 
            bool noLock = false)
        {
            return Create(new ConnectionManager(connection, 
                commandTimeout ?? new TimeSpan(0, 5, 0)), keyColumn, 
                tableName, profiler ?? new ConsoleProfiler(), noLock);
        }

        public static ITable<TEntity> Create(
            IConnectionManager connectionManager, 
            string tableName, 
            string keyColumn, 
            IProfiler profiler = null, 
            bool noLock = false)
        {
            var mapping = new EntityMapping(new AutoClassMap<TEntity>(keyColumn));
            return new Table<TEntity>(connectionManager, tableName, 
                mapping, profiler ?? new ConsoleProfiler(), noLock);
        }

        public static ITable<TEntity> Create(
            SqlConnection connection, 
            string tableName, 
            IEntityMapping entityMapping = null, 
            TimeSpan? commandTimeout = null, 
            IProfiler profiler = null, 
            bool noLock = false)
        {
            return Create(new ConnectionManager(connection, commandTimeout ?? new TimeSpan(0, 5, 0)), 
                tableName, entityMapping, profiler ?? new ConsoleProfiler(), noLock);
        }

        public static ITable<TEntity> Create(
            IConnectionManager connectionManager, 
            string tableName, 
            IEntityMapping entityMapping = null, 
            IProfiler profiler = null, 
            bool noLock = false)
        {
            return new Table<TEntity>(connectionManager, tableName, 
                entityMapping ?? new EntityMapping(new AutoClassMap<TEntity>()), 
                profiler ?? new ConsoleProfiler(), noLock);
        }

        public string Name { get; }

        // ---------------------- IOrderedQueryable Implementation -----------------

        public Expression Expression
        {
            get => _queryable.Expression;
            set => _queryable.Expression = value;
        }

        public Type ElementType => _queryable.ElementType;
        public IQueryProvider Provider => _queryable.Provider;
        public IEnumerator<TEntity> GetEnumerator() => _queryable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _queryable.GetEnumerator();
        
        // --------------------------------------------------------------------------
        
        public void Insert(TEntity entity)
        {
            var adapter = new EntityAdapter<TEntity>(entity, _mapping);
            var hasIdentityKey = _mapping.Key.KeyType == PrimaryKeyType.Integer && 
                _mapping.Key.KeyGeneration == PrimaryKeyGeneration.Server;
            var keyColumnName = _mapping.Key.ColumnName;

            if (_mapping.Key.KeyType == PrimaryKeyType.Guid && 
                _mapping.Key.KeyGeneration == PrimaryKeyGeneration.Client)
                adapter.Key = GuidComb.Create();

            var values = adapter.GetValues().Where(x => !hasIdentityKey || 
                (x.Key != keyColumnName)).ToDictionary(x => x.Key, x => x.Value);

            var insert = new Insert { HasIdentityKey = hasIdentityKey,
                Type = Model.Insert.SetType.Values, 
                Into = new Table { Name = Name},
                Values = values };

            var command = Command.Create(InsertWriter<TEntity>
                .CreateStatement(insert, _mapping), _profiler);

            if (command.Statement.Result == Statement.ResultType.None)
                command.ExecuteNonQuery(_connectionManager);
            else adapter.Key = command.ExecuteScalar(_connectionManager);
        }

        public TEntity Get<T>(T id)
        {
            var select = new Select {
                Top = 1,
                From = { Type = Data.DataType.Table, Table = new Table { Name = Name } },
                Where = CreateKeyFilter(id) };
            return _operations.ExecuteQuery<TEntity, IEnumerable<TEntity>>(select).FirstOrDefault();
        }

        public void Update(TEntity entity)
        {
            var adapter = new EntityAdapter<TEntity>(entity, _mapping);
            var keyColumnName = _mapping.Key.ColumnName;
            var values = adapter.GetValues().Where(x => x.Key != keyColumnName);
            Command.Create(UpdateWriter<TEntity>.CreateStatement(new Update(values, Name, CreateEntityKeyFilter(entity, adapter)), _mapping), _profiler).
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
            var query = QueryVisitor<TEntity>.CreateModel(source.Expression, 
                x => ((INamedQueryable) x).Name, _mapping);
            return Command.Create(DeleteWriter<TEntity>.CreateStatement(
                new Delete(Name, query.Select, true), _mapping), _profiler).
                    ExecuteNonQuery(_connectionManager);
        }

        private int Delete(Operator filter, bool multiDelete)
        {
            return Command.Create(DeleteWriter<TEntity>.CreateStatement(
                new Delete(Name, filter, multiDelete), _mapping), _profiler).
                    ExecuteNonQuery(_connectionManager);
        }

        private Operator CreateKeyFilter<T>(T id)
        {
            var field = _mapping.Key.Property.Name;
            return Operator.Create.FieldEqualsConstant(field, id);
        }

        private Operator CreateEntityKeyFilter(TEntity entity, 
            EntityAdapter<TEntity> adapter = null)
        {
            var id = (adapter ?? new EntityAdapter<TEntity>(entity, _mapping)).Key;
            var field = _mapping.Key.Property.Name;
            return Operator.Create.FieldEqualsConstant(field, id);
        }
    }
}
