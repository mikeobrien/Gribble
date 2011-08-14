using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public static class InsertWriter<TEntity>
    {
        public static Statement CreateStatement(Insert insert, IEntityMapping mapping)
        {
            var writer = new SqlWriter();
            IDictionary<string, object> parameters = null;

            writer.InsertInto.QuotedName(insert.Table.Name);
            writer.OpenBlock.Trim().FieldList(x => x.Comma.Flush(), insert.Assignment.Keys).Trim().CloseBlock.Flush();

            var resultType = Statement.ResultType.None;

            switch (insert.Type)
            {
                case Insert.InsertType.Record:
                    parameters = new Dictionary<string, object>();
                    writer.Values.OpenBlock.Trim().ParameterList(x => x.Comma.Flush(), insert.Assignment.Values.Select(x => parameters.AddWithRandomlyNamedKey(x))).
                                  Trim().CloseBlock.Flush();
                    if (insert.HasIdentityKey)
                    {
                        writer.Trim().QuerySeperator.Select.ScopeIdentity(typeof(int));
                        resultType = Statement.ResultType.Scalar;
                    }
                    break;
                case Insert.InsertType.Query:
                    var select = SelectWriter<TEntity>.CreateStatement(insert.Query, mapping);
                    parameters = select.Parameters;
                    writer.Write(select.Text);
                    break;
            }

            return new Statement(writer.ToString(), Statement.StatementType.Text, resultType, parameters);
        }
    }
}
