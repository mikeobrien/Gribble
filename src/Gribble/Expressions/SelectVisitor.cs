using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gribble.Model;

namespace Gribble.Expressions
{
    public class SelectVisitor<T> : ExpressionVisitorBase<Select>
    {
        public class QueryOperatorNotSupportedException : Exception
        {
            public QueryOperatorNotSupportedException(MethodBase method) :
                base(string.Format("Query operator '{0}({1})' not supported.", 
                        method.Name, method.GetParameters().Select(x => x.Name).Aggregate((a, i) => a + ", " + i))) { }
        }

        private readonly Func<IQueryable<T>, string> _getTableName;

        public SelectVisitor() {}

        public SelectVisitor(Func<IQueryable<T>, string> getTableName)
        {
            _getTableName = getTableName;
        }

        public static Select CreateModel(Expression expression, Func<IQueryable<T>, string> getTableName)
        {
            var select = new Select();
            new SelectVisitor<T>(getTableName).Visit(expression, select);
            return select;
        }

        protected override void VisitConstant(Context context, ConstantExpression node)
        {
            HandleQuery(context.State, node.Value, _getTableName);
            base.VisitConstant(context, node);
        }

        protected override void VisitMethodCall(Context context, MethodCallExpression node)
        {
            var emptyObject = new object();
            var select = context.State;

            if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Union(Enumerable.Empty<T>())))
                AddSourceQuery(select, CreateModel(node.GetArgument(2), _getTableName));
            else 
            {        
                // If we have unions we need to nest the following query operators as they apply to the net result of the 
                // union. This is consistent with the behavior of linq to objects.
                if (select.Source.HasQueries) select = CreateSubQuery(select);

                if (node.MatchesMethodSignature<IQueryable<T>>(x => x.CopyTo(string.Empty)) || 
                    node.MatchesMethodSignature<IQueryable<T>>(x => x.CopyTo(Queryable.Empty<T>())))
                    HandleSelectInto(select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Where(y => true))) 
                    HandleWhere(select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Distinct(y => emptyObject)))
                    HandleDistinct(select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Duplicates(y => emptyObject)))
                    HandleDuplicates(select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Duplicates(y => emptyObject, y => false)))
                    HandleDuplicates(select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Duplicates(y => emptyObject, y => emptyObject, Order.Ascending)))
                    HandleDuplicates(select, node);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.OrderBy(y => emptyObject))) 
                    HandleOrderBy(select, node, false);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.OrderByDescending(y => emptyObject))) 
                    HandleOrderBy(select, node, true);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Take(0)))
                    { select.Top = node.GetConstantArgument<int>(2); select.TopType = Select.TopValueType.Count; }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.TakePercent(0)))
                    { select.Top = node.GetConstantArgument<int>(2); select.TopType = Select.TopValueType.Percent; }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Skip(0))) 
                    select.Start = node.GetConstantArgument<int>(2) + 1;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.First())) 
                    select.First = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.First(y => true)))
                    { select.First = true; HandleWhere(select, node); }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.FirstOrDefault())) 
                    select.FirstOrDefault = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.FirstOrDefault(y => true))) 
                    { select.FirstOrDefault = true; HandleWhere(select, node); }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Count())) 
                    select.Count = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Count(y => true)))
                    { select.Count = true; HandleWhere(select, node); }
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Randomize())) 
                    select.Randomize = true;
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Intersect(Queryable.Empty<T>(), new Expression<Func<T, object>>[] { })))
                    HandleSetOperation(select, node, true, _getTableName);
                else if (node.MatchesMethodSignature<IQueryable<T>>(x => x.Except(Queryable.Empty<T>(), new Expression<Func<T, object>>[] { })))
                    HandleSetOperation(select, node, false, _getTableName);
                else throw new QueryOperatorNotSupportedException(node.Method);
            }

            VisitMethodCall(node, select, select, true, true, node.Arguments.Skip(1).ToArray());
        }

        private static void HandleQuery(Select select, object value, Func<IQueryable<T>, string> getTableName)
        {
            if (getTableName == null || !(value is IQueryable<T>)) return;

            // If we have unions we need to nest the source table as an additional source query.
            if (select.Source.HasQueries) select = CreateSubQuery(select);

            select.Source.Type = Data.DataType.Table;
            if (select.Source.Table == null) select.Source.Table = new Table();
            select.Source.Table.Name = getTableName((IQueryable<T>)value);
        }

        private static Select CreateSubQuery(Select select)
        {
            var subQuery = new Select();
            AddSourceQuery(select, subQuery);
            return subQuery;
        }

        private static void HandleSelectInto(Select select, MethodCallExpression expression)
        {
            if (select.Target.Table == null) select.Target.Table = new Table();
            select.Target.Type = Data.DataType.Table;
            if (expression.GetArgument(2).Type == typeof(string))
                select.Target.Table.Name = expression.GetConstantArgument<string>(2);
            else
            {
                var target = expression.GetConstantArgument<IQueryable<T>>(2);
                if (target is INamedQueryable) select.Target.Table.Name = ((INamedQueryable)target).Name;
                else throw new Exception("Provider only supports SelectInto type Table<T>.");
            }
        }

        private static void HandleWhere(Select select, MethodCallExpression expression)
        {
            var where = WhereVisitor<T>.CreateModel(expression.GetArgument(2));
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
            if (select.Distinct == null) select.Distinct = new List<Projection>();
            select.Distinct.Add(ProjectionVisitor<T>.CreateModel(expression.GetArgument(2)));
        }

        private static void HandleDuplicates(Select select, MethodCallExpression expression)
        {
            if (select.Duplicates == null) select.Duplicates = new Duplicates();
            select.Duplicates.DistinctField = ProjectionVisitor<T>.CreateModel(expression.GetArgument(2));
            if (expression.HasArguments(3))
            {
                select.Duplicates.Precedence = WhereVisitor<T>.CreateModel(expression.GetArgument(3));
                select.Duplicates.Grouping = Duplicates.DuplicateGrouping.Precedence;
            }
            else if (expression.HasArguments(4)) 
            {
                select.Duplicates.OrderField = ProjectionVisitor<T>.CreateModel(expression.GetArgument(3));
                select.Duplicates.Order = expression.GetConstantArgument<Order>(4);
                select.Duplicates.Grouping = Duplicates.DuplicateGrouping.OrderField;
            }
            else select.Duplicates.Grouping = Duplicates.DuplicateGrouping.DistinctField;
        }

        private static void AddSourceQuery(Select select, Select query)
        {
            if (select.Source.Queries == null) select.Source.Queries = new List<Select>();
            select.Source.Type = Data.DataType.Query;
            select.Source.Queries.Add(query);
        }

        private static void HandleSetOperation(Select select, MethodCallExpression expression, bool intersect, Func<IQueryable<T>, string> getTableName)
        {
            if (select.SetOperatons == null) select.SetOperatons = new List<SetOperation>();
            var setOperationSource = CreateModel(expression.GetArgument(2), getTableName);
            setOperationSource.Top = 1;
            setOperationSource.TopType = Select.TopValueType.Count;
            var operatorExpressions = expression.GetArgument<NewArrayExpression>(3).
                                                                       Expressions.
                                                                       Select(x => x.NodeType == ExpressionType.Quote ? ((UnaryExpression)x).Operand : x).
                                                                       Select(x => ((LambdaExpression)x).Body).
                                                                       Select(x => x.NodeType == ExpressionType.Convert ? ((UnaryExpression)x).Operand : x);
            var operators = operatorExpressions.Select(x => Operator.Create.Equal(Operand.Create.Projection(ProjectionVisitor<T>.CreateModel(x)), 
                                                                                  Operand.Create.Projection(ProjectionVisitor<T>.CreateModel(x, select.Source.Alias))));
            foreach (var @operator in operators)
                setOperationSource.Where = setOperationSource.HasWhere ? Operator.Create.Operators(setOperationSource.Where, Operator.OperatorType.And, @operator) : @operator;
            setOperationSource.Projection = operatorExpressions.Select(x => SelectProjection.Create(ProjectionVisitor<T>.CreateModel(x))).ToList();
            select.SetOperatons.Add(new SetOperation { Type = intersect ? SetOperation.OperationType.Intersect : SetOperation.OperationType.Compliment,
                                                       Select = setOperationSource });
        }

        private static void HandleOrderBy(Select select, MethodCallExpression expression, bool descending)
        {
            if (select.OrderBy == null) select.OrderBy = new List<OrderByProjection>();
            select.OrderBy.Insert(0, new OrderByProjection { Order = descending ? Model.Order.Descending : Model.Order.Ascending, 
                                                             Projection = ProjectionVisitor<T>.CreateModel(expression.GetArgument(2))});
        }
    }
}
