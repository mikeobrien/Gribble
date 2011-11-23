using System;
using System.Data;
using System.Data.SqlClient;
using NHibernate;

namespace Gribble.NHibernate
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ISession _session;
        private readonly TimeSpan _commandTimeout;

        public ConnectionManager(ISession session, TimeSpan commandTimeout)
        {
            _session = session;
            _commandTimeout = commandTimeout;
        }

        public SqlConnection Connection { get { return (SqlConnection)_session.Connection; } }

        public IDbCommand CreateCommand()
        {
            var command = new SqlCommand
                       {
                           Connection = Connection,
                           CommandTimeout = (int)_commandTimeout.TotalSeconds
                       };
            
            if (_session.Transaction.IsActive) _session.Transaction.Enlist(command);
            return command;
        }

        public void Dispose() { }
    }
}
