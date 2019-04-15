using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public static class SelectWriter<TEntity>
    {
        public static Statement CreateStatement(Select select, IEntityMapping mapping, IEnumerable<string> projectionOverride = null, bool noLock = false)
        {
            var sql = new SqlWriter();
            var parameters = new Dictionary<string, object>();

            var projection = projectionOverride ?? BuildProjection(select, mapping, parameters);
            var whereClause = BuildWhereClause(select, mapping, parameters);
            var orderByClause = select.HasOrderBy ? BuildOrderBy(select.OrderBy, mapping, parameters) : null;

            Action<SqlWriter> writeProjection = x => x.Do(projection != null, y => y.ProjectionList(z => z.Comma.Flush(), projection), y => y.Wildcard.Flush());

            if (select.Any) sql.Select.Cast().Trim().OpenBlock.Trim().Case.When.Exists.OpenBlock.Trim();

            if (select.From.IsTable || select.HasConditions)
            {
                sql.Select.Flush();

                if (select.First || select.FirstOrDefault) sql.Top(1);
                else if (select.Single) sql.Top(2);
                else if (select.HasTop && !select.HasStart) 
                    sql.Do(select.TopType == Select.TopValueType.Count, x => x.Top(select.Top), x => x.TopPercent(select.Top));

                if (select.Count) sql.CountWildcard.Flush();
                else sql.Do(writeProjection);
            
                sql.From.Flush();

                if (select.HasStart)
                {
                    sql.OpenBlock.Trim().Select.Do(writeProjection).Trim().Comma.
                        RowNumber().Over.OpenBlock.Trim().OrderBy.
                        Do(select.HasOrderBy, x => x.Write(orderByClause), x => x.Do(projection != null, writeProjection, y => y.QuotedName(mapping.Key.GetColumnName()))).Trim().
                        CloseBlock.As.RowNumberAlias.From.Flush();
                }

                if (select.HasDuplicates)
                {
                    var duplicateProjection = BuildProjection(select.Duplicates.Distinct, mapping, parameters);
                    sql.OpenBlock.Trim().Select.Do(writeProjection).Trim().Comma.
                        RowNumber().Over.OpenBlock.Trim().Partition.By.Write(duplicateProjection).OrderBy.Write(BuildOrderBy(select.Duplicates.OrderBy, mapping, parameters)).
                        Trim().CloseBlock.As.PartitionAlias.From.Flush();
                } 
                else if (select.HasDistinct)
                {
                    var distinctProjection = BuildProjections(select.Distinct.Select(x => x.Projection), mapping, parameters);
                    sql.OpenBlock.Trim().Select.Do(writeProjection).Trim().Comma.
                        RowNumber().Over.OpenBlock.Trim().Partition.By.ExpressionList(z => z.Comma.Flush(), distinctProjection).OrderBy.
                        Write(BuildOrderBy(select.Distinct.Any(x => x.HasOrder) ? select.Distinct.Where(x => x.HasOrder).Select(x => x.Order) : 
                            select.Distinct.Select(x => new OrderBy { 
                                            Type = OrderBy.SourceType.Projection, 
                                            Projection = x.Projection, 
                                            Order = Order.Ascending }), mapping, parameters)).Trim().
                        CloseBlock.As.PartitionAlias.From.Flush();
                }
            }

            switch (select.From.Type)
            {
                case Data.DataType.Table: 
                    sql.QuotedName(select.From.Table.Name).Write(select.From.Alias);
                    if (noLock) sql.With(x => x.NoLock.Flush());
                    break;
                case Data.DataType.Query:
                    var first = true;
                    if (select.HasConditions) sql.OpenBlock.Trim().Flush();
                    foreach (var subQuery in select.From.Queries.Select(x => CreateStatement(x, mapping, projection, noLock)))
                    {
                        sql.Do(!first, x => x.Union.Flush()).Write(subQuery.Text).Flush();
                        parameters.AddRange(subQuery.Parameters);
                        first = false;
                    }
                    if (select.HasConditions) sql.Trim().CloseBlock.As.Write(select.From.Alias).Flush();
                    break;
            }

            if (select.From.IsTable || select.HasConditions)
            {
                if (select.HasWhere || select.HasSetOperations) sql.Where.Write(whereClause).Flush();

                if (select.Randomize) sql.OrderBy.NewId();
                // The reason why we dont do an order by if there is a start is because the order by is 
                // already specified in the row number definition. So we dont need to specify it again.
                else if (select.HasOrderBy && !select.HasStart && !select.HasDistinct) sql.OrderBy.Write(orderByClause);

                if (select.HasDuplicates)
                    sql.Trim().CloseBlock.As.Write(select.From.Alias).Where.PartitionAlias.GreaterThan.Value(1, SqlDbType.Int).Flush();
                else if (select.HasDistinct) 
                    sql.Trim().CloseBlock.As.Write(select.From.Alias).Where.PartitionAlias.Equal.Value(1, SqlDbType.Int).Flush();

                if (select.HasStart)
                {
                    sql.Trim().CloseBlock.As.Write(select.From.Alias).Where.RowNumberAlias.Flush();
                    if (select.HasTop && select.HasStart) sql.Between(select.Start, select.Start + (select.Top - 1));
                    else sql.GreaterThanOrEqual.Value(select.Start, SqlDbType.Int);
                }
            }

            if (select.Any) sql.Trim().CloseBlock.Then.Value(1, SqlDbType.Bit).Else.Value(0, SqlDbType.Bit)
                .End.As.Write(DataTypes.Bit.SqlName).Trim().CloseBlock.Flush();

            return new Statement(sql.ToString(), Statement.StatementType.Text, GetResultType(select), parameters);
        }

        public static Statement.ResultType GetResultType(Select select)
        {
            if (select.First || select.Single) return Statement.ResultType.Single;
            if (select.FirstOrDefault) return Statement.ResultType.SingleOrNone;
            if (select.Count || select.Any) return Statement.ResultType.Scalar;
            return Statement.ResultType.Multiple;
        }

        public static IEnumerable<string> BuildProjection(Select select, IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            if (!select.HasProjection) return null;
            return select.Projection.Select(x => BuildSelectProjection(x, mapping, parameters));
        }

        private static string BuildSelectProjection(SelectProjection projection, 
            IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            var text = BuildProjection(projection.Projection, mapping, parameters);
            if (!projection.Alias.IsNullOrEmpty())
                text += new SqlWriter().Space().As.QuotedName(projection.Alias).ToString(false);
            return text;
        }

        private static IEnumerable<string> BuildProjections(IEnumerable<Projection> projections, 
            IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            return projections.Select(x => BuildProjection(x, mapping, parameters));
        }

        private static string BuildProjection(Projection projection, IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            var projectionStatement = ProjectionWriter<TEntity>.CreateStatement(projection, mapping);
            parameters.AddRange(projectionStatement.Parameters);
            return projectionStatement.Text;
        }

        private static string BuildWhereClause(Select select, IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            if (!select.HasWhere && !select.HasSetOperations) return null;

            var writer = new SqlWriter();

            if (select.HasWhere) writer.Write(BuildOperators(select.Where, mapping, parameters));

            if (select.HasSetOperations)
            {
                foreach (var setOperation in select.SetOperatons)
                {
                    var statement = CreateStatement(setOperation.Select, mapping);
                    parameters.AddRange(statement.Parameters);
                    if (!writer.Empty) writer.And.Flush();
                    if (setOperation.Type == SetOperation.OperationType.Compliment) writer.Not.Flush();
                    writer.Exists.OpenBlock.Trim().Write(statement.Text).Trim().CloseBlock.Flush();
                }
            }

            return writer.ToString();
        }

        private static string BuildOperators(Operator @operator, IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            var writer = new SqlWriter();
            var statement = WhereWriter<TEntity>.CreateStatement(@operator, mapping);
            parameters.AddRange(statement.Parameters);
            writer.Write(statement.Text);
            return writer.ToString();
        }

        private static string BuildOrderBy(IEnumerable<OrderBy> orderBy, IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            if (orderBy == null || !orderBy.Any()) return "";
            var writer = new SqlWriter();
            orderBy.Select((x, i) => new { Last = i == orderBy.Count() - 1, OrderBy = x }).ToList().ForEach(x =>
                {
                    if (x.OrderBy.Type == OrderBy.SourceType.Operator)
                        writer.Case.When.Write(BuildOperators(x.OrderBy.Operator, mapping, parameters)).Then
                            .Value(1, SqlDbType.Int).Else.Value(0, SqlDbType.Int).End.Flush();
                    else writer.Write(BuildProjection(x.OrderBy.Projection, mapping, parameters));
                    writer.Do(x.OrderBy.Order == Order.Descending, y => y.Descending.Flush(), y => y.Ascending.Flush()).Do(!x.Last, y => y.Trim().Comma.Flush());
                });
            return writer.ToString();
        }
    }
}
