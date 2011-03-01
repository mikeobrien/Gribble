using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;
using Gribble.Statements;

namespace Gribble.TransactSql
{
    public static class UpdateWriter<TEntity>
    {
        public static Statement CreateStatement(Update update, IEntityMapping mapping)
        {
            var writer = new SqlWriter();
            var parameters = new Dictionary<string, object>();

            writer.Update.QuotedName(update.Table.Name).Set.
                ParameterAssignmentList(x => x.Comma.Flush(), update.Assignment.ToDictionary(x => x.Key, x => parameters.AddWithRandomlyNamedKey(x.Value)));

            var where = WhereWriter<TEntity>.CreateStatement(update.Where, mapping);
            writer.Where.Write(where.Text);
            parameters.AddRange(where.Parameters);

            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None, parameters);
        }
    }
}
