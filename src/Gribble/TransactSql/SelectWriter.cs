using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gribble.Mapping;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public static class SelectWriter<TEntity>
    {
        public static Statement CreateStatement(Select select, IEntityMapping mapping, IEnumerable<string> projectionOverride = null)
        {
            var sql = new SqlWriter();
            var parameters = new Dictionary<string, object>();

            var projection = projectionOverride ?? BuildProjection(select, mapping, parameters);
            var whereClause = BuildWhereClause(select, mapping, parameters);
            var orderByClause = BuildOrderByClause(select, mapping, parameters);

            Action<SqlWriter> writeProjection = x => x.Do(projection != null, y => y.FieldList(z => z.Comma.Flush(), projection), y => y.Wildcard.Flush());

            if (select.Source.IsTable || select.HasConditions)
            {
                sql.Select.Flush();

                if (select.First || select.FirstOrDefault) sql.Top(1);
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
                    var duplicateProjection = BuildProjection(select.Duplicates.DistinctField, mapping, parameters);
                    sql.OpenBlock.Trim().Select.Do(writeProjection).Trim().Comma.
                        RowNumber().Over.OpenBlock.Trim().Partition.By.Write(duplicateProjection).OrderBy.Flush();
                    if (select.Duplicates.Grouping == Duplicates.DuplicateGrouping.Precedence)
                    {
                        var duplicatePrecedence = BuildOperators(select.Duplicates.Precedence, mapping, parameters);
                        sql.Case.When.Write(duplicatePrecedence).Then.Value(1, SqlDbType.Int).Else.Value(0, SqlDbType.Int).End.Flush();
                    }
                    else if (select.Duplicates.Grouping == Duplicates.DuplicateGrouping.OrderField)
                        sql.Write(BuildProjection(select.Duplicates.OrderField, mapping, parameters)).
                            Do(select.Duplicates.Order == Order.Descending, x => x.Descending.Flush());
                    else sql.Write(duplicateProjection);
                    sql.Trim().CloseBlock.As.PartitionAlias.From.Flush();
                } 
                else if (select.HasDistinct)
                {
                    var distinctProjection = BuildProjections(select.Distinct, mapping, parameters);
                    sql.OpenBlock.Trim().Select.Do(writeProjection).Trim().Comma.
                        RowNumber().Over.OpenBlock.Trim().Partition.By.ExpressionList(z => z.Comma.Flush(), distinctProjection).OrderBy.
                        Do(select.HasOrderBy, x => x.Write(orderByClause), x => x.Do(projection != null, writeProjection, y => y.QuotedName(mapping.Key.GetColumnName()))).Trim().
                        CloseBlock.As.PartitionAlias.From.Flush();
                }
            }

            switch (select.Source.Type)
            {
                case Data.DataType.Table: sql.QuotedName(select.Source.Table.Name).Write(select.Source.Alias); break;
                case Data.DataType.Query:
                    var first = true;
                    if (select.HasConditions) sql.OpenBlock.Trim().Flush();
                    foreach (var subQuery in select.Source.Queries.Select(x => CreateStatement(x, mapping, projection)))
                    {
                        sql.Do(!first, x => x.Union.Flush()).Write(subQuery.Text).Flush();
                        parameters.AddRange(subQuery.Parameters);
                        first = false;
                    }
                    if (select.HasConditions) sql.Trim().CloseBlock.As.Write(select.Source.Alias).Flush();
                    break;
            }

            if (select.Source.IsTable || select.HasConditions)
            {
                if (select.HasWhere || select.HasSetOperations) sql.Where.Write(whereClause).Flush();

                if (select.Randomize) sql.OrderBy.NewId();
                // The reason why we dont do an order by if there is a start is because the order by is 
                // already specified in the row number definition. So we dont need to specify it again.
                else if (select.HasOrderBy && !select.HasStart && !select.HasDistinct) sql.OrderBy.Write(orderByClause);

                if (select.HasDuplicates)
                    sql.Trim().CloseBlock.As.Write(select.Source.Alias).Where.PartitionAlias.GreaterThan.Value(1, SqlDbType.Int).Flush();
                else if (select.HasDistinct) 
                    sql.Trim().CloseBlock.As.Write(select.Source.Alias).Where.PartitionAlias.Equal.Value(1, SqlDbType.Int).Flush();

                if (select.HasStart)
                {
                    sql.Trim().CloseBlock.As.Write(select.Source.Alias).Where.RowNumberAlias.Flush();
                    if (select.HasTop && select.HasStart) sql.Between(select.Start, select.Start + (select.Top - 1));
                    else sql.GreaterThanOrEqual.Value(select.Start, SqlDbType.Int);
                }
            }

            return new Statement(sql.ToString(), Statement.StatementType.Text, GetResultType(select), parameters);
        }

        public static Statement.ResultType GetResultType(Select select)
        {
            if (select.First) return Statement.ResultType.Single;
            if (select.FirstOrDefault) return Statement.ResultType.SingleOrNone;
            if (select.Count) return Statement.ResultType.Scalar;
            return Statement.ResultType.Multiple;
        }

        private static IEnumerable<string> BuildProjection(Select select, IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            if (!select.HasProjection) return null;
            return BuildProjections(select.Projection.Select(x => x.Projection), mapping, parameters);
        }

        private static IEnumerable<string> BuildProjections(IEnumerable<Projection> projections, IEntityMapping mapping, IDictionary<string, object> parameters)
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

        private static string BuildOrderByClause(Select select, IEntityMapping mapping, IDictionary<string, object> parameters)
        {
            if (!select.HasOrderBy) return null;

            var projections = select.OrderBy.Select(x => new { Statement = ProjectionWriter<TEntity>.CreateStatement(x.Projection, mapping), x.Order });
            parameters.AddRange(projections.SelectMany(x => x.Statement.Parameters));
            Func<string, Order, Action<SqlWriter>> writeOrderByExpression = 
                (text, order) => y => y.Write(text).Do(order == Order.Ascending, z => z.Ascending.Flush(), z => z.Descending.Flush());
            return SqlWriter.CreateWriter().ExpressionList(x => x.Comma.Flush(), projections.Select(x => writeOrderByExpression(x.Statement.Text, x.Order))).ToString();
        }
    }
}
