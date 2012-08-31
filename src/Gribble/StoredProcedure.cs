using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Gribble.Mapping;
using Gribble.TransactSql;

namespace Gribble
{
    public class StoredProcedure : IStoredProcedure
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IProfiler _profiler;
        private readonly EntityMappingCollection _map;

        public StoredProcedure(IConnectionManager connectionManager, EntityMappingCollection map) : 
            this(connectionManager, map, null) { }

        internal StoredProcedure(IConnectionManager connectionManager, EntityMappingCollection map, IProfiler profiler)
        {
            _connectionManager = connectionManager;
            _profiler = profiler;
            _map = map;
        }

        public static IStoredProcedure Create(SqlConnection connection, string keyColumn, TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, commandTimeout ?? new TimeSpan(0, 5, 0)), keyColumn, profiler);
        }

        public static IStoredProcedure Create(IConnectionManager connectionManager, string keyColumn, IProfiler profiler = null)
        {
            return new StoredProcedure(connectionManager, new EntityMappingCollection(new IClassMap[] { new GuidKeyEntityMap(keyColumn), new IntKeyEntityMap(keyColumn) }), profiler ?? new ConsoleProfiler());
        }

        public static IStoredProcedure Create(SqlConnection connection, EntityMappingCollection mappingCollection, TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, commandTimeout ?? new TimeSpan(0, 5, 0)), mappingCollection, profiler);
        }

        public static IStoredProcedure Create(IConnectionManager connectionManager, EntityMappingCollection mappingCollection, IProfiler profiler = null)
        {
            return new StoredProcedure(connectionManager, mappingCollection, profiler ?? new ConsoleProfiler());
        }

        public void Execute(string name)
        { Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.None), _profiler).ExecuteNonQuery(_connectionManager); }

        public void Execute(string name, Dictionary<string, object> parameters)
        { Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.None), _profiler).ExecuteNonQuery(_connectionManager); }

        public T ExecuteScalar<T>(string name)
        { return Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.Scalar), _profiler).ExecuteScalar<T>(_connectionManager); }

        public T ExecuteScalar<T>(string name, Dictionary<string, object> parameters)
        { return Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.Scalar), _profiler).ExecuteScalar<T>(_connectionManager); }

        public TEntity ExecuteSingle<TEntity>(string name)
        { return Load<TEntity, TEntity>(Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.Single), _profiler)); }

        public TEntity ExecuteSingle<TEntity>(string name, Dictionary<string, object> parameters)
        { return Load<TEntity, TEntity>(Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.Single), _profiler)); }

        public TEntity ExecuteSingleOrNone<TEntity>(string name)
        { return Load<TEntity, TEntity>(Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.SingleOrNone), _profiler)); }

        public TEntity ExecuteSingleOrNone<TEntity>(string name, Dictionary<string, object> parameters)
        { return Load<TEntity, TEntity>(Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.SingleOrNone), _profiler)); }

        public IEnumerable<TEntity> ExecuteMany<TEntity>(string name)
        { return Load<TEntity, IEnumerable<TEntity>>(Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.Multiple), _profiler)); }

        public IEnumerable<TEntity> ExecuteMany<TEntity>(string name, Dictionary<string, object> parameters)
        { return Load<TEntity, IEnumerable<TEntity>>(Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.Multiple), _profiler)); }

        private TResult Load<TEntity, TResult>(Command command)
        { return (TResult)new Loader<TEntity>(command, _map.GetEntityMapping<TEntity>()).Execute(_connectionManager); }
    }
}
