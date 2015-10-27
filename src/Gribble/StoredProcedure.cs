using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.TransactSql;

namespace Gribble
{
    public interface IStoredProcedure
    {
        bool Exists(string name);
        int Execute(string name, object parameters = null);
        TReturn Execute<TReturn>(string name, object parameters = null);
        T ExecuteScalar<T>(string name, object parameters = null);
        TEntity ExecuteSingle<TEntity>(string name, object parameters = null) where TEntity : class;
        TEntity ExecuteSingleOrNone<TEntity>(string name, object parameters = null) where TEntity : class;
        IEnumerable<TEntity> ExecuteMany<TEntity>(string name, object parameters = null) where TEntity : class;
    }

    public class StoredProcedure : IStoredProcedure
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IProfiler _profiler;
        private readonly EntityMappingCollection _map;

        public StoredProcedure(IConnectionManager connectionManager, 
            EntityMappingCollection map, IProfiler profiler)
        {
            _connectionManager = connectionManager;
            _profiler = profiler;
            _map = map;
        }

        public static IStoredProcedure Create(SqlConnection connection, 
            TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, 
                commandTimeout ?? new TimeSpan(0, 5, 0)), profiler);
        }

        public static IStoredProcedure Create(IConnectionManager connectionManager, 
            IProfiler profiler = null)
        {
            return new StoredProcedure(connectionManager, 
                new EntityMappingCollection(Enumerable.Empty<IClassMap>()), 
                profiler ?? new ConsoleProfiler());
        }

        public static IStoredProcedure Create(SqlConnection connection, 
            string keyColumn, TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, 
                commandTimeout ?? new TimeSpan(0, 5, 0)), 
                keyColumn, profiler);
        }

        public static IStoredProcedure Create(IConnectionManager connectionManager, 
            string keyColumn, IProfiler profiler = null)
        {
            return new StoredProcedure(connectionManager, new EntityMappingCollection(
                new IClassMap[] { new GuidKeyEntityMap(keyColumn), new IntKeyEntityMap(keyColumn) }), 
                profiler ?? new ConsoleProfiler());
        }

        public static IStoredProcedure Create(SqlConnection connection, 
            EntityMappingCollection mappingCollection, 
            TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, 
                commandTimeout ?? new TimeSpan(0, 5, 0)), 
                mappingCollection, profiler);
        }

        public static IStoredProcedure Create(IConnectionManager connectionManager, 
            EntityMappingCollection mappingCollection, IProfiler profiler = null)
        {
            return new StoredProcedure(connectionManager, mappingCollection, 
                profiler ?? new ConsoleProfiler());
        }

        public bool Exists(string name)
        {
            return Command.Create(SchemaWriter.CreateProcedureExistsStatement(name), _profiler)
                .ExecuteScalar<bool>(_connectionManager);
        }

        public int Execute(string name, object parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name, 
                parameters.ToDictionary(), Statement.ResultType.None), _profiler)
                    .ExecuteNonQuery(_connectionManager);
        }

        public TReturn Execute<TReturn>(string name, object parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name, 
                parameters.ToDictionary(), Statement.ResultType.None), _profiler)
                    .ExecuteNonQuery<TReturn>(_connectionManager);
        }

        public T ExecuteScalar<T>(string name, object parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name, 
                parameters.ToDictionary(), Statement.ResultType.Scalar), _profiler)
                    .ExecuteScalar<T>(_connectionManager);
        }

        public TEntity ExecuteSingle<TEntity>(string name, 
            object parameters = null) where TEntity : class 
        {
            return Load<TEntity, TEntity>(Command.Create(StatementWriter
                .CreateStoredProcedure(name, parameters.ToDictionary(), 
                    Statement.ResultType.Single), _profiler));
        }

        public TEntity ExecuteSingleOrNone<TEntity>(string name, 
            object parameters = null) where TEntity : class 
        {
            return Load<TEntity, TEntity>(Command.Create(StatementWriter
                .CreateStoredProcedure(name, parameters.ToDictionary(), 
                    Statement.ResultType.SingleOrNone), _profiler));
        }

        public IEnumerable<TEntity> ExecuteMany<TEntity>(string name, 
            object parameters = null) where TEntity : class 
        {
            return Load<TEntity, IEnumerable<TEntity>>(Command.Create(
                StatementWriter.CreateStoredProcedure(name, parameters.ToDictionary(), 
                    Statement.ResultType.Multiple), _profiler));
        }

        private TResult Load<TEntity, TResult>(Command command) where TEntity : class 
        {
            return (TResult)new Loader<TEntity>(command, _map.GetEntityMapping<TEntity>())
                .Execute(_connectionManager);
        }
    }
}
