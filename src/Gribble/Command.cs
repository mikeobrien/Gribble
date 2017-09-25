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
                base($"Error executing sql: {exception.Message}", exception)
            { Statement = statement; }

            public Statement Statement { get; }
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

        public Statement Statement { get; }

        public IDataReader ExecuteReader(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    return command.ExecuteReader();
                }
            });
        }

        public T ExecuteScalar<T>(IConnectionManager connectionManager)
        {
            return (T)ExecuteScalar(connectionManager);
        }

        public object ExecuteScalar(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    return command.ExecuteScalar().FromDb<object>();
                }
            });
        }

        public int ExecuteNonQuery(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    return command.ExecuteNonQuery();
                }
            });
        }

        public TReturn ExecuteNonQuery<TReturn>(IConnectionManager connectionManager)
        {
            return Execute(() => {
                const string parameterName = "@__return__";
                using (var command = CreateCommand(connectionManager))
                {
                    command.Parameters.Add(new SqlParameter {
                            ParameterName = parameterName, Direction = ParameterDirection.ReturnValue });
                    command.ExecuteNonQuery();
                    return (TReturn)command.Parameters[parameterName].Value;
                }
            });
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
        
        public DataSet ExecuteDataSet(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    var dataSet = new DataSet();
                    new SqlDataAdapter(command).Fill(dataSet);
                    return dataSet;
                }
            });
        }
        public DataTable ExecuteDataTable(string tableName, IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    var table = new DataTable(tableName);
                    new SqlDataAdapter(command).Fill(table);
                    return table;
                } 
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
                message.Append(" " + statement.Parameters.Select(x => x.Key).Aggregate((a, i) => $"{a}, {i}"));
            profiler.Write(message.ToString());
        }
    }
}
