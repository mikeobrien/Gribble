using System.Data;
using System.Data.SqlClient;
using NHibernate;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace Gribble.NHibernate
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ISession _session;
        private readonly int _timeout;

        public ConnectionManager(ISession session)
        {
            _session = session;
            // Indirectly get a hold of the configured command timeout. Could not find another way to get at this value.
            _timeout = _session.GetSessionImplementation().Factory.ConnectionProvider.Driver
                               .GenerateCommand(CommandType.Text, new SqlString(), new SqlType[] { }).CommandTimeout;
        }

        public SqlConnection Connection { get { return (SqlConnection)_session.Connection; } }

        public IDbCommand CreateCommand()
        {
            var command = _session.Connection.CreateCommand();
            command.CommandTimeout = _timeout;
            _session.Transaction.Enlist(command);
            return command;
        }

        public void Dispose() { }
    }
}
