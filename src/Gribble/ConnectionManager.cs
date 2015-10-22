using System;
using System.Data.SqlClient;

namespace Gribble
{
    public interface IConnectionManager : IDisposable
    {
        SqlCommand CreateCommand();
    }

    public class ConnectionManager : IConnectionManager
    {
        private readonly Lazy<SqlConnection> _connection;
        private readonly TimeSpan _commandTimeout;

        public ConnectionManager(SqlConnection connection, TimeSpan? commandTimeout = null)
        {
            _connection = new Lazy<SqlConnection>(() => connection);
            _commandTimeout = commandTimeout ?? new TimeSpan(0, 5, 0);
        }

        public ConnectionManager(string connectionString, TimeSpan? commandTimeout = null)
        {
            _connection = new Lazy<SqlConnection>(() =>
                {
                    var connection = new SqlConnection(connectionString);
                    connection.Open();
                    return connection;
                });
            _commandTimeout = commandTimeout ?? new TimeSpan(0, 5, 0);
        }
        
        public SqlCommand CreateCommand()
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
