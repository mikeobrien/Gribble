using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Gribble.Extensions;
using Gribble.TransactSql;

namespace Gribble
{
    public class Command
    {
        public class SqlException : Exception
        {
            public SqlException(Exception exception, Statement statement) : 
                base(string.Format("Error executing sql: {0}", exception.Message), exception)
            { Statement = statement; }

            public Statement Statement { get; private set; }
        }

        private readonly IProfiler _profiler;

        public Command(Statement statement, IProfiler profiler)
        {
            Statement = statement;
            _profiler = profiler;
        }

        public static Command Create(Statement statement, IProfiler profiler)
        {
            return new Command(statement, profiler);
        }

        public Statement Statement { get; private set; }

        public IDataReader ExecuteReader(IConnectionManager connectionManager)
        { return Execute(() => CreateCommand(connectionManager).ExecuteReader()); }

        public T ExecuteScalar<T>(IConnectionManager connectionManager)
        { return (T)ExecuteScalar(connectionManager); }

        public object ExecuteScalar(IConnectionManager connectionManager)
        { return Execute(() => CreateCommand(connectionManager).ExecuteScalar().FromDb<object>()); }

        public int ExecuteNonQuery(IConnectionManager connectionManager)
        {
            return Execute(() => CreateCommand(connectionManager).ExecuteNonQuery());
        }

        public TReturn ExecuteNonQuery<TReturn>(IConnectionManager connectionManager)
        {
            return Execute(() => {
                const string parameterName = "@__return__";
                var command = CreateCommand(connectionManager);
                command.Parameters.Add(new SqlParameter {
                        ParameterName = parameterName, Direction = ParameterDirection.ReturnValue });
                command.ExecuteNonQuery();
                return (TReturn)command.Parameters[parameterName].Value;
            });
        }

        private SqlCommand CreateCommand(IConnectionManager connectionManager)
        {
            var command = connectionManager.CreateCommand();
            command.CommandText = Statement.Text;
            command.CommandType = Statement.Type == Statement.StatementType.Text ? CommandType.Text : CommandType.StoredProcedure;
            Statement.Parameters.Select(x => new SqlParameter(x.Key, x.Value ?? DBNull.Value)).ToList().ForEach(y => command.Parameters.Add(y));
            return command;
        }

        public IEnumerable<T> ExecuteEnumerable<T>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => (T)x[0]);
        }

        public IEnumerable<T> ExecuteEnumerable<T>(IConnectionManager connectionManager, Func<IDataReader, T> createItem)
        {
            return Execute(() =>
            {
                var values = new List<T>();
                using (var reader = CreateCommand(connectionManager).ExecuteReader())
                    while (reader.Read()) values.Add(createItem(reader));
                return values;
            });
        }

        private T Execute<T>(Func<T> command)
        {
            try
            {
                if (_profiler != null) ProfileCommand(Statement, _profiler);
                return command();
            }
            catch (Exception exception)
            {
                throw new SqlException(exception, Statement);
            }
        }

        private static void ProfileCommand(Statement statement, IProfiler profiler)
        {
            var message = new StringBuilder();
            message.Append(statement.Text);
            if (statement.Type == Statement.StatementType.StoredProcedure && statement.Parameters.Count > 0)
                message.Append(" " + statement.Parameters.Select(x => x.Key).Aggregate((a, i) => string.Format("{0}, {1}", a, i)));
            profiler.Write(message.ToString());
        }
    }
}
