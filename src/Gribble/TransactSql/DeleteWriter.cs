using Gribble.Mapping;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public static class DeleteWriter<TEntity>
    {
        public static Statement CreateStatement(Delete delete, IEntityMapping mapping)
        {
            Statement statement = null;
            var writer = SqlWriter.CreateWriter().Delete;

            if (!delete.AllowMultiple) writer.Top(1);

            writer.From.QuotedName(delete.Table.Name);

            switch (delete.Filter)
            {
                case Delete.FilterType.Where:   
                    statement = WhereWriter<TEntity>.CreateStatement(delete.Where, mapping);
                    writer.Where.Write(statement.Text);
                    break;
                case Delete.FilterType.Select:
                    var keyColumn = mapping.Key.GetColumnName();
                    statement = SelectWriter<TEntity>.CreateStatement(delete.Select, mapping);
                    writer.Where.QuotedName(keyColumn).In.OpenBlock.Trim().Select.QuotedName(keyColumn).From.OpenBlock.Trim().
                        Write(statement.Text).Trim().CloseBlock.As.SubQueryAlias.Trim().CloseBlock.Flush();
                    break;
            }

            return new Statement(writer.ToString(), Statement.StatementType.Text, Statement.ResultType.None, statement.Parameters);
        }
    }
}
