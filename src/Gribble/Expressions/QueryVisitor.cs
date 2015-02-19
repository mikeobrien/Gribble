using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gribble.Extensions;
using Gribble.Model;
using Queryable = Gribble.Extensions.Queryable;

namespace Gribble.Expressions
{
    public class QueryVisitor<T> : ExpressionVisitorBase<Query>
    {
        public class QueryOperatorNotSupportedException : Exception
        {
            public QueryOperatorNotSupportedException(MethodBase method) :
                base(string.Format("Query operator '{0}({1})' not supported.", 
                        method.Name, method.GetParameters().Select(x => x.Name).Aggregate((a, i) => a + ", " + i))) { }
        }

        private readonly Func<IQueryable<T>, string> _getTableName;

        public QueryVisitor() {}

        public QueryVisitor(Func<IQueryable<T>, string> getTableName)
        {
            _getTableName = getTableName;
        }

        public static Query CreateModel(Expression expression, Func<IQueryable<T>, string> getTableName)
        {
            var query = new Query {Select = new Select()};
            new QueryVisitor<T>(getTableName).Visit(expression, query);
            return query;
        }

        protected override void VisitConstant(Context context, ConstantExpression node)
        {
            HandleQuery(context.State, node.Value, _getTableName);
            base.VisitConstant(context, node);
        }

        protected override void VisitMethodCall(Context context, MethodCallExpression node)
        {
            var query = context.State;

            if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Union(Enumerable.Empty<T>())))
                AddSourceQuery(query.Select, CreateModel(node.ArgumentAt(2), _getTableName).Select);
            else if (query.Select.From.HasQueries)
            {
                // If we have unions we need to nest subsequent query operators otherwise they would apply to the 
                // net result of the union. This is consistent with the behavior of linq to objects.
                AddSourceQuery(query.Select, CreateModel(node, _getTableName).Select);
                return;
            }
            else
            {        
                if (node.MatchesMethodSignature<IQueryable<T>>(x => x.CopyTo(Queryable.Empty<T>())))
                    HandleSelectInto(query, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Where(y => true))) 
                    HandleWhere(query.Select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Distinct(y => y)))
                    HandleDistinct(query.Select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Distinct(y => y, y => y, Order.Ascending)))
                    HandleDistinct(query.Select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Duplicates(y => y)))
                    HandleDuplicates(query.Select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Duplicates(y => y, y => y, Order.Ascending)))
                    HandleDuplicates(query.Select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Duplicates(y => y, y => y, Order.Ascending, y => y, Order.Ascending)))
                    HandleDuplicates(query.Select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.OrderBy(y => y))) 
                    HandleOrderBy(query.Select, node, false);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.OrderByDescending(y => y))) 
                    HandleOrderBy(query.Select, node, true);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Take(0)))
                    { query.Select.Top = node.ConstantArgumentAt<int>(2); query.Select.TopType = Select.TopValueType.Count; }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.TakePercent(0)))
                    { query.Select.Top = node.ConstantArgumentAt<int>(2); query.Select.TopType = Select.TopValueType.Percent; }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Skip(0))) 
                    query.Select.Start = node.ConstantArgumentAt<int>(2) + 1;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.First())) 
                    query.Select.First = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.First(y => true)))
                    { query.Select.First = true; HandleWhere(query.Select, node); }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.FirstOrDefault())) 
                    query.Select.FirstOrDefault = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.FirstOrDefault(y => true))) 
                    { query.Select.FirstOrDefault = true; HandleWhere(query.Select, node); }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Any())) 
                    query.Select.Any = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Any(y => true)))
                    { query.Select.Any = true; HandleWhere(query.Select, node); }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Count())) 
                    query.Select.Count = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Count(y => true)))
                    { query.Select.Count = true; HandleWhere(query.Select, node); }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Randomize())) 
                    query.Select.Randomize = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Intersect(
                    Queryable.Empty<T>(), new Expression<Func<T, object>>[] { })))
                    HandleSetOperation(query.Select, node, true, _getTableName);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Except(
                    Queryable.Empty<T>(), new Expression<Func<T, object>>[] { })))
                    HandleSetOperation(query.Select, node, false, _getTableName);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.SyncWith(Queryable.Empty<T>(), y => y, 
                    SyncFields.Exclude, new Expression<Func<T, object>>[] { })))
                    HandleSync(query, node);
                else throw new QueryOperatorNotSupportedException(node.Method);
            }

            VisitMethodCall(node, query, query, true, true, node.Arguments.Skip(1).ToArray());
        }

        private static void HandleQuery(Query query, object value, Func<IQueryable<T>, string> getTableName)
        {
            if (getTableName == null || !(value is IQueryable<T>)) return;

            // If we have unions we need to nest the source table as an additional source query.
            var select = query.Select.From.HasQueries ? CreateSubQuery(query.Select) : query.Select;

            select.From.Type = Data.DataType.Table;
            if (select.From.Table == null) select.From.Table = new Table();
            select.From.Table.Name = getTableName((IQueryable<T>)value);
        }

        private static Select CreateSubQuery(Select select)
        {
            var subQuery = new Select();
            AddSourceQuery(select, subQuery);
            return subQuery;
        }

        private static void AddSourceQuery(Select select, Select query)
        {
            if (select.From.Queries == null) select.From.Queries = new List<Select>();
            select.From.Type = Data.DataType.Query;
            select.From.Queries.Add(query);
        }

        private static void HandleSelectInto(Query query, MethodCallExpression expression)
        {
            query.Operation = Query.OperationType.CopyTo;
            query.CopyTo = new Insert { Type = Insert.SetType.Query, Query = query.Select, Into = new Table() };
            var target = expression.ConstantArgumentAt<IQueryable<T>>(2);
            if (target is INamedQueryable) query.CopyTo.Into.Name = ((INamedQueryable)target).Name;
            else throw new Exception("Provider only supports SelectInto type Table<T>.");
        }

        private void HandleSync(Query target, MethodCallExpression expression)
        {
            target.Operation = Query.OperationType.SyncWith;
            var fields = expression.ArgumentAt<NewArrayExpression>(5).Expressions.Select(x => x.GetLambdaBody().StripConversion());
            var exclude = expression.ConstantArgumentAt<SyncFields>(4) == SyncFields.Exclude;
            var source = CreateModel(expression.ArgumentAt(2), _getTableName).Select;
            if (!exclude)
            {
                source.Projection = fields.Select(x => new SelectProjection { Projection = ProjectionVisitor<T>.CreateModel(x, source.From.Alias)}).ToList();
                target.Select.Projection = fields.Select(x => new SelectProjection { Projection = ProjectionVisitor<T>.CreateModel(x, target.Select.From.Alias)}).ToList();
            }
            target.SyncWith = new Sync {
                Target = target.Select,
                Source = source,
                SourceKey = ProjectionVisitor<T>.CreateModel(expression.ArgumentAt(3), source.From.Alias),
                TargetKey = ProjectionVisitor<T>.CreateModel(expression.ArgumentAt(3), target.Select.From.Alias),
                ExcludedFields = exclude ? fields.Select(x => ProjectionVisitor<T>.CreateModel(x).Field).ToList() : new List<Field>()
            };
        }

        private static void HandleWhere(Select select, MethodCallExpression expression)
        {
            var where = WhereVisitor<T>.CreateModel(expression.ArgumentAt(2), select.From.Alias);
            if (!select.HasWhere) select.Where = where;
            else
            {
                var @operator = Operator.Create.ByType(Operator.OperatorType.And);
                @operator.LeftOperand = Operand.Create.Operator(select.Where);
                @operator.RightOperand = Operand.Create.Operator(where);
                select.Where = @operator;
            }
        }

        private static void HandleDistinct(Select select, MethodCallExpression expression)
        {
            if (select.Distinct == null) select.Distinct = new List<Distinct>();
            select.Distinct.Insert(0, new Distinct {
                Projection = ProjectionVisitor<T>.CreateModel(expression.ArgumentAt(2)),
                Order = expression.HasArguments(4) ? new OrderBy {
                            Type = OrderBy.SourceType.Projection,
                            Projection = ProjectionVisitor<T>.CreateModel(expression.ArgumentAt(3)),
                            Order = expression.ConstantArgumentAt<Order>(4) } : null});
        }

        private static void HandleDuplicates(Select select, MethodCallExpression expression)
        {
            if (select.Duplicates == null) select.Duplicates = new Duplicates();
            select.Duplicates.Distinct = ProjectionVisitor<T>.CreateModel(expression.ArgumentAt(2));
            if (expression.Arguments.Count == 2) select.Duplicates.OrderBy.Add(new OrderBy {
                                Type = OrderBy.SourceType.Projection, 
                                Projection = select.Duplicates.Distinct, 
                                Order = Order.Ascending
                            });
            else Enumerable.Range(3, expression.Arguments.Count - 2)
                    .Where(x => x % 2 == 1)
                    .Select(x => new { Index = x, IsProjection = ProjectionVisitor<T>.IsProjection(expression.ArgumentAt(x).GetLambdaBody()) })
                    .ToList().ForEach(x => select.Duplicates.OrderBy.Add(new OrderBy {
                                Type = x.IsProjection ? OrderBy.SourceType.Projection : OrderBy.SourceType.Operator,
                                Projection = x.IsProjection ? ProjectionVisitor<T>.CreateModel(expression.ArgumentAt(x.Index)) : null,
                                Operator = !x.IsProjection ? WhereVisitor<T>.CreateModel(expression.ArgumentAt(x.Index), select.From.Alias) : null,
                                Order = expression.ConstantArgumentAt<Order>(x.Index + 1)
                            }));
        }

        private static void HandleSetOperation(Select select, MethodCallExpression expression, bool intersect, Func<IQueryable<T>, string> getTableName)
        {
            if (select.SetOperatons == null) select.SetOperatons = new List<SetOperation>();
            var setOperationSource = CreateModel(expression.ArgumentAt(2), getTableName).Select;
            setOperationSource.Top = 1;
            setOperationSource.TopType = Select.TopValueType.Count;
            var operatorExpressions = expression.ArgumentAt<NewArrayExpression>(3).
                                                                       Expressions.
                                                                       Select(x => x.NodeType == ExpressionType.Quote ? ((UnaryExpression)x).Operand : x).
                                                                       Select(x => ((LambdaExpression)x).Body).
                                                                       Select(x => x.NodeType == ExpressionType.Convert ? ((UnaryExpression)x).Operand : x);
            var operators = operatorExpressions.Select(x => Operator.Create.Equal(Operand.Create.Projection(ProjectionVisitor<T>.CreateModel(x)), 
                                                                                  Operand.Create.Projection(ProjectionVisitor<T>.CreateModel(x, select.From.Alias))));
            foreach (var @operator in operators)
                setOperationSource.Where = setOperationSource.HasWhere ? Operator.Create.Operators(setOperationSource.Where, Operator.OperatorType.And, @operator) : @operator;
            setOperationSource.Projection = operatorExpressions.Select(x => SelectProjection.Create(ProjectionVisitor<T>.CreateModel(x))).ToList();
            select.SetOperatons.Add(new SetOperation { Type = intersect ? SetOperation.OperationType.Intersect : SetOperation.OperationType.Compliment,
                                                       Select = setOperationSource });
        }

        private static void HandleOrderBy(Select select, MethodCallExpression expression, bool descending)
        {
            if (select.OrderBy == null) select.OrderBy = new List<OrderBy>();
            select.OrderBy.Insert(0, new OrderBy { Order = descending ? Order.Descending : Order.Ascending, 
                                                   Projection = ProjectionVisitor<T>.CreateModel(expression.ArgumentAt(2)),
                                                   Type = OrderBy.SourceType.Projection });
        }
    }
}
