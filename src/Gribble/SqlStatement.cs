using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.TransactSql;

namespace Gribble
{
    public interface ISqlStatement
    {
        int Execute(string commandText, object parameters = null);
        T ExecuteScalar<T>(string commandText, object parameters = null);
        TEntity ExecuteSingle<TEntity>(string commandText, object parameters = null) where TEntity : class;
        TEntity ExecuteSingleOrNone<TEntity>(string commandText, object parameters = null) where TEntity : class;
        IEnumerable<TEntity> ExecuteMany<TEntity>(string commandText, object parameters = null) where TEntity : class;
    }

    public class SqlStatement : ISqlStatement
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IProfiler _profiler;
        private readonly EntityMappingCollection _map;

        public SqlStatement(IConnectionManager connectionManager, 
            EntityMappingCollection map, IProfiler profiler)
        {
            _connectionManager = connectionManager;
            _profiler = profiler;
            _map = map;
        }

        public static ISqlStatement Create(SqlConnection connection, 
            TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, commandTimeout ?? 
                new TimeSpan(0, 5, 0)), profiler);
        }

        public static ISqlStatement Create(IConnectionManager connectionManager, IProfiler profiler = null)
        {
            return new SqlStatement(connectionManager, new EntityMappingCollection(
                Enumerable.Empty<IClassMap>()), profiler ?? new ConsoleProfiler());
        }

        public static ISqlStatement Create(SqlConnection connection, string keyColumn, 
            TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, commandTimeout ?? 
                new TimeSpan(0, 5, 0)), keyColumn, profiler);
        }

        public static ISqlStatement Create(IConnectionManager connectionManager, 
            string keyColumn, IProfiler profiler = null)
        {
            return new SqlStatement(connectionManager, new EntityMappingCollection(
                new IClassMap[] { new GuidKeyEntityMap(keyColumn), new IntKeyEntityMap(keyColumn) }), 
                profiler ?? new ConsoleProfiler());
        }

        public static ISqlStatement Create(SqlConnection connection, 
            EntityMappingCollection mappingCollection, 
            TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, commandTimeout ?? 
                new TimeSpan(0, 5, 0)), mappingCollection, profiler);
        }

        public static ISqlStatement Create(IConnectionManager connectionManager, 
            EntityMappingCollection mappingCollection, IProfiler profiler = null)
        {
            return new SqlStatement(connectionManager, mappingCollection, profiler ?? new ConsoleProfiler());
        }

        public int Execute(string commandText, object parameters = null)
        {
            return Execute(commandText, parameters.ToDictionary());
        }

        public T ExecuteScalar<T>(string commandText, object parameters = null)
        {
            return ExecuteScalar<T>(commandText, parameters.ToDictionary());
        }

        public TEntity ExecuteSingle<TEntity>(string commandText,
            object parameters = null) where TEntity : class
        {
            return ExecuteSingle<TEntity>(commandText, parameters.ToDictionary());
        }

        public TEntity ExecuteSingleOrNone<TEntity>(string commandText,
            object parameters = null) where TEntity : class
        {
            return ExecuteSingleOrNone<TEntity>(commandText, parameters.ToDictionary());
        }

        public IEnumerable<TEntity> ExecuteMany<TEntity>(string commandText,
            object parameters = null) where TEntity : class
        {
            return ExecuteMany<TEntity>(commandText, parameters.ToDictionary());
        }

        public int Execute(string commandText, IDictionary<string, object> parameters)
        {
            return ExecuteBatches(commandText, parameters, x => ExecuteNonQuery(x, parameters));
        }

        public T ExecuteScalar<T>(string commandText, IDictionary<string, object> parameters)
        {
            return ExecuteBatches(commandText, parameters, x =>
                Command.Create(StatementWriter.CreateStatement(x,
                        parameters, Statement.ResultType.Scalar), _profiler)
                    .ExecuteScalar<T>(_connectionManager));
        }

        public TEntity ExecuteSingle<TEntity>(string commandText,
            IDictionary<string, object> parameters) where TEntity : class
        {
            return ExecuteBatches(commandText, parameters, x =>
                Load<TEntity, TEntity>(Command.Create(StatementWriter.CreateStatement(x,
                    parameters, Statement.ResultType.Single), _profiler)));
        }

        public TEntity ExecuteSingleOrNone<TEntity>(string commandText,
            IDictionary<string, object> parameters) where TEntity : class
        {
            return ExecuteBatches(commandText, parameters, x =>
                Load<TEntity, TEntity>(Command.Create(StatementWriter.CreateStatement(x,
                    parameters, Statement.ResultType.SingleOrNone), _profiler)));
        }

        public IEnumerable<TEntity> ExecuteMany<TEntity>(string commandText,
            IDictionary<string, object> parameters) where TEntity : class
        {
            return ExecuteBatches(commandText, parameters, x =>
                Load<TEntity, IEnumerable<TEntity>>(
                    Command.Create(StatementWriter.CreateStatement(x, parameters,
                        Statement.ResultType.Multiple), _profiler)));
        }

        private TResult Load<TEntity, TResult>(Command command) where TEntity : class 
        {
            return (TResult)new Loader<TEntity>(command, _map.GetEntityMapping<TEntity>())
                .Execute(_connectionManager);
        }

        private T ExecuteBatches<T>(string commandText, IDictionary<string, object> parameters, Func<string, T> command)
        {
            var commands = SplitBatches(commandText);
            if (commands.Length > 1)
                commands.Take(commands.Length - 1).ToList()
                    .ForEach(x => ExecuteNonQuery(x, parameters));
            return command(commands.Last());
        }

        private int ExecuteNonQuery(string commandText, IDictionary<string, object> parameters = null)
        {
            return Command.Create(StatementWriter.CreateStatement(commandText,
                    parameters, Statement.ResultType.None), _profiler)
                    .ExecuteNonQuery(_connectionManager);
        }

        private static string[] SplitBatches(string commandText)
        {
            return commandText.Split(new[] { "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
