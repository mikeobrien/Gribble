using System;
using System.Data;
using Gribble.TransactSql;

namespace Gribble
{
    public class SqlException : Exception
    {
        public SqlException(Exception exception, Statement statement) :
            base($"Error executing sql: {exception.Message}", exception)
        {
            Statement = statement;
            Sql = GetCommandSql(statement);
        }

        public Statement Statement { get; }
        public string Sql { get; }
        
        private string GetCommandSql(Statement statement)
        {
            if (statement == null) return null;

            var sql = statement.Text;

            foreach (var parameter in statement.Parameters)
            {
                sql = sql.Replace(parameter.Key, FormatValue(parameter.Value));
            }

            return sql;
        }

        private static string FormatValue(object value)
        {
            if (value == DBNull.Value || value == null)
                return "NULL";

            switch (value.GetType().GetSqlType())
            {
                case SqlDbType.Char:
                case SqlDbType.DateTime:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Text:
                case SqlDbType.Timestamp:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    return $"'{value}'";

                case SqlDbType.Bit: return value.Equals(true) ? "1" : "0";

                default: return value.ToString();
            }
        }
    }
}