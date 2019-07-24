using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.TransactSql;

namespace Gribble
{
    public interface IStoredProcedure
    {
        bool Exists(string name);
        int ExecuteNonQuery(string name, IDictionary<string, object> parameters = null);
        Task<int> ExecuteNonQueryAsync(string name, IDictionary<string, object> parameters = null);
        TReturn ExecuteReturn<TReturn>(string name, IDictionary<string, object> parameters = null);
        Task<TReturn> ExecuteReturnAsync<TReturn>(string name, IDictionary<string, object> parameters = null);
        T ExecuteScalar<T>(string name, IDictionary<string, object> parameters = null);
        Task<T> ExecuteScalarAsync<T>(string name, IDictionary<string, object> parameters = null);
        TEntity ExecuteSingle<TEntity>(string name, 
            IDictionary<string, object> parameters = null) where TEntity : class;
        TEntity ExecuteSingleOrNone<TEntity>(string name, 
            IDictionary<string, object> parameters = null) where TEntity : class;
        IEnumerable<TEntity> ExecuteMany<TEntity>(string name, 
            IDictionary<string, object> parameters = null) where TEntity : class;
        DataSet ExecuteDataSet(string name,
            IDictionary<string, object> parameters = null);
        DataTable ExecuteDataTable(string tableName, string name,
            IDictionary<string, object> parameters = null);
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

        public int ExecuteNonQuery(string name, IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name,
                    parameters, Statement.ResultType.None), _profiler)
                .ExecuteNonQuery(_connectionManager);
        }

        public Task<int> ExecuteNonQueryAsync(string name, IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name,
                    parameters, Statement.ResultType.None), _profiler)
                .ExecuteNonQueryAsync(_connectionManager);
        }

        public TReturn ExecuteReturn<TReturn>(string name, IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name,
                    parameters, Statement.ResultType.None), _profiler)
                .ExecuteReturn<TReturn>(_connectionManager);
        }

        public Task<TReturn> ExecuteReturnAsync<TReturn>(string name, IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name,
                    parameters, Statement.ResultType.None), _profiler)
                .ExecuteReturnAsync<TReturn>(_connectionManager);
        }

        public T ExecuteScalar<T>(string name, IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name,
                    parameters, Statement.ResultType.Scalar), _profiler)
                .ExecuteScalar<T>(_connectionManager);
        }

        public Task<T> ExecuteScalarAsync<T>(string name, IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name,
                    parameters, Statement.ResultType.Scalar), _profiler)
                .ExecuteScalarAsync<T>(_connectionManager);
        }

        public TEntity ExecuteSingle<TEntity>(string name,
            IDictionary<string, object> parameters = null) where TEntity : class
        {
            return Load<TEntity, TEntity>(Command.Create(StatementWriter
                .CreateStoredProcedure(name, parameters,
                    Statement.ResultType.Single), _profiler));
        }

        public TEntity ExecuteSingleOrNone<TEntity>(string name,
            IDictionary<string, object> parameters = null) where TEntity : class
        {
            return Load<TEntity, TEntity>(Command.Create(StatementWriter
                .CreateStoredProcedure(name, parameters,
                    Statement.ResultType.SingleOrNone), _profiler));
        }

        public IEnumerable<TEntity> ExecuteMany<TEntity>(string name,
            IDictionary<string, object> parameters = null) where TEntity : class
        {
            return Load<TEntity, IEnumerable<TEntity>>(Command.Create(
                StatementWriter.CreateStoredProcedure(name, parameters,
                    Statement.ResultType.Multiple), _profiler));
        }

        public DataSet ExecuteDataSet(string name,
            IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name,
                    parameters, Statement.ResultType.Multiple), _profiler)
                .ExecuteDataSet(_connectionManager);
        }

        public DataTable ExecuteDataTable(string tableName, string name,
            IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStoredProcedure(name, 
                    parameters, Statement.ResultType.Multiple), _profiler)
                .ExecuteDataTable(tableName, _connectionManager);
        }

        private TResult Load<TEntity, TResult>(Command command) where TEntity : class 
        {
            return (TResult)new Loader<TEntity>(command, _map.GetEntityMapping<TEntity>())
                .Load(_connectionManager);
        }
    }

    public static class IStoredProcedureExtensions
    {
        public static int ExecuteNonQuery(this IStoredProcedure storedProcedure,
            string name, object parameters)
        {
            return storedProcedure.ExecuteNonQuery(name, parameters.AsDictionary());
        }

        public static TReturn ExecuteReturn<TReturn>(this IStoredProcedure storedProcedure,
            string name, object parameters)
        {
            return storedProcedure.ExecuteReturn<TReturn>(name, parameters.AsDictionary());
        }

        public static T ExecuteScalar<T>(this IStoredProcedure storedProcedure,
            string name, object parameters)
        {
            return storedProcedure.ExecuteScalar<T>(name, parameters.AsDictionary());
        }

        public static TEntity ExecuteSingle<TEntity>(
            this IStoredProcedure storedProcedure, string name,
            object parameters) where TEntity : class
        {
            return storedProcedure.ExecuteSingle<TEntity>(name, parameters.AsDictionary());
        }

        public static TEntity ExecuteSingleOrNone<TEntity>(
            this IStoredProcedure storedProcedure,
            string name, object parameters) where TEntity : class
        {
            return storedProcedure.ExecuteSingleOrNone<TEntity>(name, parameters.AsDictionary());
        }

        public static IEnumerable<TEntity> ExecuteMany<TEntity>(
            this IStoredProcedure storedProcedure,
            string name, object parameters) where TEntity : class
        {
            return storedProcedure.ExecuteMany<TEntity>(name, parameters.AsDictionary());
        }

        public static IEnumerable<IDictionary<string, object>> ExecuteDictionary(
            this IStoredProcedure storedProcedure, string name, IDictionary<string, object> parameters = null)
        {
            return storedProcedure.ExecuteMany<IDictionary<string, object>>(name, parameters);
        }

        public static IEnumerable<IDictionary<string, object>> ExecuteDictionary(
            this IStoredProcedure storedProcedure, string name, object parameters)
        {
            return storedProcedure.ExecuteDictionary(name, parameters.AsDictionary());
        }

        public static DataSet ExecuteDataSet(this IStoredProcedure storedProcedure,
            string name, object parameters)
        {
            return storedProcedure.ExecuteDataSet(name, parameters.AsDictionary());
        }

        public static DataTable ExecuteDataTable(this IStoredProcedure storedProcedure,
            string tableName, string name, object parameters)
        {
            return storedProcedure.ExecuteDataTable(tableName, name, parameters.AsDictionary());
        }
    }
}
