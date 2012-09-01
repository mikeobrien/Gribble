using System.Collections.Generic;

namespace Gribble.TransactSql
{
    public static class StoredProcedureWriter
    {
        public static Statement CreateStatement(string name, IDictionary<string, object> parameters, Statement.ResultType result)
        {
            return new Statement(name, Statement.StatementType.StoredProcedure, result, parameters);
        }
    }
}
