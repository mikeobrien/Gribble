using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

namespace Gribble
{
    public class ConnectionManager : IConnectionManager
    {
        private const string _defaultProviderName = "System.Data.SqlClient";

        private readonly Lazy<IDbConnection> _connection;
        private readonly TimeSpan _commandTimeout;
        public static TimeSpan? CommandTimeout { private get; set; }

        public ConnectionManager(IDbConnection connection, TimeSpan? commandTimeout = null)
        {
            _connection = new Lazy<IDbConnection>(() => connection);
            _commandTimeout = commandTimeout ?? (CommandTimeout ?? new TimeSpan(0, 5, 0));
        }

        public ConnectionManager(string connectionString, TimeSpan? commandTimeout = null)
        {
            var factory = DbProviderFactories.GetFactory(_defaultProviderName);
            _connection = new Lazy<IDbConnection>(() =>
                        {
                            var connection = factory.CreateConnection();
                             connection.ConnectionString = connectionString;
                            connection.Open();
                            return connection;
                        });
            _commandTimeout = commandTimeout ?? (CommandTimeout ?? new TimeSpan(0, 5, 0));
        }

        public ConnectionManager(string connectionString, string providerName, TimeSpan? commandTimeout = null)
        {
            var factory = DbProviderFactories.GetFactory(string.IsNullOrEmpty(providerName) ? _defaultProviderName : providerName);
            _connection = new Lazy<IDbConnection>(() =>
            {
                var connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;
                connection.Open();
                return connection;
            });
            _commandTimeout = commandTimeout ?? (CommandTimeout ?? new TimeSpan(0, 5, 0));
        }

        public ConnectionManager(string connectionString, DbProviderFactory provider, TimeSpan? commandTimeout = null)
        {
            _connection = new Lazy<IDbConnection>(() =>
            {
                var connection = provider.CreateConnection();
                connection.ConnectionString = connectionString;
                connection.Open();
                return connection;
            });
            _commandTimeout = commandTimeout ?? (CommandTimeout ?? new TimeSpan(0, 5, 0));
        }

        public IDbConnection Connection { get { return _connection.Value; } }

        public IDbCommand CreateCommand()
        {
            var command = _connection.Value.CreateCommand();
            command.CommandTimeout = (int) _commandTimeout.TotalSeconds;
            return command;
        }

        public void Dispose()
        {
            if (_connection.IsValueCreated) _connection.Value.Dispose();
        }
    }
}
