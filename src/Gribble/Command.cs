using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gribble.Extensions;
using Gribble.TransactSql;

namespace Gribble
{
    public class Command
    {
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

        public Task<SqlDataReader> ExecuteReaderAsync(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    return command.ExecuteReaderAsync();
                }
            });
        }

        public T ExecuteScalar<T>(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    return command.ExecuteScalar().FromDb<T>();
                }
            });
        }

        public Task<T> ExecuteScalarAsync<T>(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    return command.ExecuteScalarAsync().FromDbAsync<T>();
                }
            });
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

        public Task<object> ExecuteScalarAsync(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    return command.ExecuteScalarAsync().FromDbAsync<object>();
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

        public Task<int> ExecuteNonQueryAsync(IConnectionManager connectionManager)
        {
            return Execute(() =>
            {
                using (var command = CreateCommand(connectionManager))
                {
                    return command.ExecuteNonQueryAsync();
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

        public Task<TReturn> ExecuteNonQueryAsync<TReturn>(IConnectionManager connectionManager)
        {
            return Execute(async () => {
                const string parameterName = "@__return__";
                using (var command = CreateCommand(connectionManager))
                {
                    command.Parameters.Add(new SqlParameter {
                        ParameterName = parameterName, Direction = ParameterDirection.ReturnValue });
                    await command.ExecuteNonQueryAsync();
                    return (TReturn)command.Parameters[parameterName].Value;
                }
            });
        }

        public IEnumerable<T> ExecuteEnumerable<T>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerable(connectionManager, x => x[0].FromDb<T>());
        }

        public Task<IEnumerable<T>> ExecuteEnumerableAsync<T>(IConnectionManager connectionManager)
        {
            return ExecuteEnumerableAsync(connectionManager, x => x[0].FromDb<T>());
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

        public Task<IEnumerable<T>> ExecuteEnumerableAsync<T>(
            IConnectionManager connectionManager, Func<IDataReader, T> createItem)
        {
            return Execute(async () =>
            {
                var values = new List<T>();
                using (var reader = await CreateCommand(connectionManager).ExecuteReaderAsync())
                    while (reader.Read()) values.Add(createItem(reader));
                return (IEnumerable<T>)values;
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
            Statement.Parameters.Select(x =>
                {
                    var parameter = new SqlParameter(x.Key, x.Value ?? DBNull.Value);
                    if (x.Value is byte[]) parameter.DbType = DbType.Binary;
                    return parameter;
                }).ToList()
                .ForEach(y => command.Parameters.Add(y));
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
