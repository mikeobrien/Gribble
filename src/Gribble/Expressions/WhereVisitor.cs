using System;
using System.Linq.Expressions;
using Gribble.Model;

namespace Gribble.Expressions
{
    public class WhereVisitor<T> : ExpressionVisitorBase<Action<Operand>>
    {
        public static Operator CreateModel(Expression expression)
        {
            var visitor = new WhereVisitor<T>();
            Operator where = null;
            visitor.Visit(expression, x => where = x.Operator);
            return where;
        }

        protected override void VisitUnary(Context context, UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    var notOperand = Operand.Create.Operator(
                        Operator.Create.RightEquals(
                            Operand.Create.Projection(Projection.Create.Constant(false))
                            )
                        );

                    context.State(notOperand);
                    VisitUnary(node, x => notOperand.Operator.LeftOperand = x);
                    break;
                case ExpressionType.Convert: HandleExpression(context, node); return;
                case ExpressionType.Quote: Visit(context, node.Operand); break;
                default : throw new OperatorNotSupportedException(node.NodeType);
            }
        }

        protected override void VisitBinary(Context context, BinaryExpression node)
        {
            Operator.OperatorType @operator;

            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And: @operator = Operator.OperatorType.And; break;
                case ExpressionType.OrElse:
                case ExpressionType.Or: @operator = Operator.OperatorType.Or; break;
                case ExpressionType.Equal: @operator = Operator.OperatorType.Equal; break;
                case ExpressionType.NotEqual: @operator = Operator.OperatorType.NotEqual; break;
                case ExpressionType.LessThan: @operator = Operator.OperatorType.LessThan; break;
                case ExpressionType.LessThanOrEqual: @operator = Operator.OperatorType.LessThanOrEqual; break;
                case ExpressionType.GreaterThan: @operator = Operator.OperatorType.GreaterThan; break;
                case ExpressionType.GreaterThanOrEqual: @operator = Operator.OperatorType.GreaterThanOrEqual; break;
                case ExpressionType.Add: @operator = Operator.OperatorType.Add; break;
                case ExpressionType.Subtract: @operator = Operator.OperatorType.Subtract; break;
                case ExpressionType.Multiply: @operator = Operator.OperatorType.Multiply; break;
                case ExpressionType.Divide: @operator = Operator.OperatorType.Divide; break;
                case ExpressionType.Modulo: @operator = Operator.OperatorType.Modulo; break;
                case ExpressionType.Coalesce: HandleExpression(context, node); return;
                default: throw new OperatorNotSupportedException(node.NodeType);
            }

            var operand = Operand.Create.Operator(Operator.Create.ByType(@operator));
            context.State(operand);
            
            VisitBinary(node, 
                x => operand.Operator.LeftOperand = x,
                null,
                x => operand.Operator.RightOperand = x,
                true,
                false,
                true);
        }

        protected override void VisitLambda(Context context, LambdaExpression node)
        {
            Visit(context, node.Body);
        }

        protected override void VisitMember(Context context, MemberExpression node)
        {
            HandleExpression(context, node);
        }

        protected override void VisitMethodCall(Context context, MethodCallExpression node)
        {
            HandleExpression(context, node);
        }

        protected override void VisitConstant(Context context, ConstantExpression node)
        {
            HandleExpression(context, node);
        }

        private static void HandleExpression(Context context, Expression expression)
        {
            Operand operand;
            if (!context.HasParent || context.Parent.IsBinaryLogicalOperator())
                operand = Operand.Create.Operator(
                           Operator.Create.Equal(
                                 Operand.Create.Projection(ProjectionVisitor<T>.CreateModel(expression)),
                                 Operand.Create.Projection(Projection.Create.Constant(true)
                               )
                           )
                       );    
            else operand = Operand.Create.Projection(ProjectionVisitor<T>.CreateModel(expression));
            
            context.State(operand);
        }
    }
}
