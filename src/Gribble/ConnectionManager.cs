using System;
using System.Data;
using System.Data.SqlClient;

namespace Gribble
{
    public class ConnectionManager : IDisposable
    {
        private readonly Lazy<SqlConnection> _connection;
        private readonly TimeSpan _commandTimeout;

        public ConnectionManager(SqlConnection connection, TimeSpan commandTimeout)
        {
            _connection = new Lazy<SqlConnection>(() => connection);
            _commandTimeout = commandTimeout;
        }

        public ConnectionManager(string connectionString, TimeSpan commandTimeout)
        {
            _connection = new Lazy<SqlConnection>(() =>
                        {
                            var connection = new SqlConnection(connectionString);
                            connection.Open();
                            return connection;
                        });
            _commandTimeout = commandTimeout;
        }

        public SqlConnection Connection { get { return _connection.Value; } }

        public IDbCommand CreateCommand()
        {
            return new SqlCommand
                       {
                           Connection = _connection.Value,
                           CommandTimeout = (int)_commandTimeout.TotalSeconds
                       };
        }

        public void Dispose()
        {
            if (_connection.IsValueCreated) _connection.Value.Dispose();
        }
    }
}
