using Gribble.Mapping;
using Gribble.Statements;

namespace Gribble.TransactSql
{
    public static class DeleteWriter<TEntity>
    {
        public static Statement CreateStatement(Delete delete, IEntityMapping mapping)
        {
            var writer = SqlWriter.CreateWriter().Delete;

            if (!delete.AllowMultiple) writer.Top(1);

            writer.From.QuotedName(delete.Table.Name);

            var whereStatement = WhereWriter<TEntity>.CreateStatement(delete.Where, mapping);
            writer.Where.Write(whereStatement.Text);

            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None, whereStatement.Parameters);
        }
    }
}
