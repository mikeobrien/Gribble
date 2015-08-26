using System.Collections.Generic;
using System.Linq;
using Gribble.Extensions;
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

            writer.InsertInto.QuotedName(insert.Into.Name);

            var resultType = Statement.ResultType.None;

            switch (insert.Type)
            {
                case Insert.SetType.Values:
                    writer.OpenBlock.Trim().FieldList(x => x.Comma.Flush(), insert.Values.Keys).Trim().CloseBlock.Flush();
                    parameters = new Dictionary<string, object>();
                    writer.Values.OpenBlock.Trim().ParameterList(x => x.Comma.Flush(), insert.Values.Values.Select(x => parameters.AddWithUniquelyNamedKey(x))).
                                  Trim().CloseBlock.Flush();
                    if (insert.HasIdentityKey)
                    {
                        writer.Trim().QuerySeperator.Select.ScopeIdentity(typeof(int));
                        resultType = Statement.ResultType.Scalar;
                    }
                    break;
                case Insert.SetType.Query:
                    var select = SelectWriter<TEntity>.CreateStatement(insert.Query, mapping);
                    parameters = select.Parameters;
                    writer.OpenBlock.Trim().FieldList(x => x.Comma.Flush(), SelectWriter<TEntity>.BuildProjection(insert.Query, mapping, parameters)).Trim().CloseBlock.Flush();
                    writer.Write(select.Text);
                    break;
            }

            return new Statement(writer.ToString(), Statement.StatementType.Text, resultType, parameters);
        }
    }
}
