using System.Collections.Generic;
using System.Linq;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public static class UpdateWriter<TEntity>
    {
        public static Statement CreateStatement(Update update, IEntityMapping mapping, string alias = null)
        {
            var writer = new SqlWriter();
            var parameters = new Dictionary<string, object>();

            writer.Update.QuotedName(alias ?? update.Table.Name);
                
            writer.Set.ParameterAssignmentList(x => x.Comma.Flush(), update.Assignment
                .ToDictionary(x => x.Key, x => parameters.AddWithUniquelyNamedKey(x.Value)));

            if (alias != null) writer.From.QuotedName(update.Table.Name).QuotedName(alias);

            var where = WhereWriter<TEntity>.CreateStatement(update.Where, mapping);
            writer.Where.Write(where.Text);
            parameters.AddRange(where.Parameters);

            return new Statement(writer.ToString(), Statement.StatementType.Text, 
                Statement.ResultType.None, parameters);
        }
    }
}