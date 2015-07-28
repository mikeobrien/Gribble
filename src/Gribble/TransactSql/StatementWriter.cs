using System.Collections.Generic;

namespace Gribble.TransactSql
{
    public static class StatementWriter
    {
        public static Statement CreateStoredProcedure(
            string name, IDictionary<string, object> parameters, 
            Statement.ResultType result)
        {
            return new Statement(name, 
                Statement.StatementType.StoredProcedure, 
                result, parameters);
        }

        public static Statement CreateStatement(
            string name, IDictionary<string, object> parameters,
            Statement.ResultType result)
        {
            return new Statement(name,
                Statement.StatementType.Text,
                result, parameters);
        }
    }
}
