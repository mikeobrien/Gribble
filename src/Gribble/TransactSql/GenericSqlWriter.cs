using System.Collections.Generic;

namespace Gribble.TransactSql
{
    public static class GenericSqlWriter
    {
        public static Statement CreateStatement(string sql, IDictionary<string, object> parameters, Statement.ResultType result)
        {
            return new Statement(sql, Statement.StatementType.Text, result, parameters);
        }
    }
}
