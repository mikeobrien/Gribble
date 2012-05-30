using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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
        { return Execute(() => CreateCommand(connectionManager).ExecuteScalar()); }

        public IEnumerable<T> ExecuteEnumerable<T>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => (T)x[0]);
        }

        public IEnumerable<Tuple<T1, T2>> ExecuteEnumerable<T1, T2>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => new Tuple<T1, T2>((T1)x[0], (T2)x[1]));
        }

        public IEnumerable<Tuple<T1, T2, T3>> ExecuteEnumerable<T1, T2, T3>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => new Tuple<T1, T2, T3>((T1)x[0], (T2)x[1], (T3)x[2]));
        }

        public IEnumerable<Tuple<T1, T2, T3, T4>> ExecuteEnumerable<T1, T2, T3, T4>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => new Tuple<T1, T2, T3, T4>((T1)x[0], (T2)x[1], (T3)x[2], (T4)x[3]));
        }

        public IEnumerable<Tuple<T1, T2, T3, T4, T5>> ExecuteEnumerable<T1, T2, T3, T4, T5>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => new Tuple<T1, T2, T3, T4, T5>((T1)x[0], (T2)x[1], (T3)x[2], (T4)x[3], (T5)x[4]));
        }

        public IEnumerable<Tuple<T1, T2, T3, T4, T5, T6>> ExecuteEnumerable<T1, T2, T3, T4, T5, T6>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => new Tuple<T1, T2, T3, T4, T5, T6>((T1)x[0], (T2)x[1], (T3)x[2], (T4)x[3], (T5)x[4], (T6)x[5]));
        }

        public IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7>> ExecuteEnumerable<T1, T2, T3, T4, T5, T6, T7>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => new Tuple<T1, T2, T3, T4, T5, T6, T7>((T1)x[0], (T2)x[1], (T3)x[2], (T4)x[3], (T5)x[4], (T6)x[5], (T7)x[6]));
        }

        public IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>> ExecuteEnumerable<T1, T2, T3, T4, T5, T6, T7, T8>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>((T1) x[0], (T2) x[1], (T3) x[2], (T4) x[3], (T5) x[4], (T6) x[5], (T7) x[6], new Tuple<T8>((T8) x[7])));
        }

        public int ExecuteNonQuery(IConnectionManager connectionManager)
        {
            return Execute(() => CreateCommand(connectionManager).ExecuteNonQuery());
        }

        private IDbCommand CreateCommand(IConnectionManager connectionManager)
        {
            var command = connectionManager.CreateCommand();
            command.CommandText = Statement.Text;
            command.CommandType = Statement.Type == Statement.StatementType.Text ? CommandType.Text : CommandType.StoredProcedure;
            Statement.Parameters.Select(x => new SqlParameter(x.Key, x.Value ?? DBNull.Value)).ToList().ForEach(y => command.Parameters.Add(y));
            return command;
        }

        private IEnumerable<T> ExecuteEnumerable<T>(IConnectionManager connectionManager, Func<IDataReader, T> createItem)
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
