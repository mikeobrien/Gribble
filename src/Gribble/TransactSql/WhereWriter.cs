using System.Collections.Generic;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.Model;

namespace Gribble.TransactSql
{
    public class WhereWriter<TEntity>
    {
        private SqlWriter _sql;
        private Dictionary<string, object> _parameters;
        private readonly IEntityMapping _mapping;

        public WhereWriter(IEntityMapping mapping)
        {
            _mapping = mapping;
        }

        public static Statement CreateStatement(Operator @operator, IEntityMapping mapping)
        {
            var writer = new WhereWriter<TEntity>(mapping);
            return writer.Write(@operator);
        }

        public Statement Write(Operator @operator)
        {
            _sql = new SqlWriter();
            _parameters = new Dictionary<string, object>();
            VisitOperator(@operator, null);
            return new Statement(_sql.ToString(), Statement.StatementType.Text, _parameters);
        }

        private void VisitOperator(Operator @operator, Operator parent)
        {
            EnsureConstantIsOnRight(@operator);
            if (IsLikeExpression(@operator.LeftOperand)) VisitLikeExpression(@operator);
            else VisitExpression(@operator, parent);
        }

        private void VisitExpression(Operator @operator, Operator parent)
        {
            var isNotNull = @operator.Type == Operator.OperatorType.NotEqual && !IsRightOperandNullConstant(@operator);
            if (IsComparisonOperator(parent) && !IsMathOperator(@operator)) _sql.Case.When.Flush();

            _sql.OpenBlock.Trim();
            if (isNotNull) _sql.OpenBlock.Trim();

            VisitOperand(@operator.LeftOperand, @operator);
            switch (@operator.Type)
            {
                case Operator.OperatorType.And: _sql.And.Flush(); break;
                case Operator.OperatorType.Or: _sql.Or.Flush(); break;
                case Operator.OperatorType.Equal: if (IsRightOperandNullConstant(@operator)) _sql.Is.Flush(); else _sql.Equal.Flush(); break;
                case Operator.OperatorType.NotEqual: if (IsRightOperandNullConstant(@operator)) _sql.Is.Not.Flush(); else _sql.NotEqual.Flush(); break;
                case Operator.OperatorType.GreaterThan: _sql.GreaterThan.Flush(); break;
                case Operator.OperatorType.GreaterThanOrEqual: _sql.GreaterThanOrEqual.Flush(); break;
                case Operator.OperatorType.LessThan: _sql.LessThan.Flush(); break;
                case Operator.OperatorType.LessThanOrEqual: _sql.LessThanOrEqual.Flush(); break;
                case Operator.OperatorType.Add: _sql.Plus.Flush(); break;
                case Operator.OperatorType.Subtract: _sql.Minus.Flush(); break;
                case Operator.OperatorType.Multiply: _sql.Multiply.Flush(); break;
                case Operator.OperatorType.Divide: _sql.Divide.Flush(); break;
                case Operator.OperatorType.Modulo: _sql.Modulo.Flush(); break;
            }
            VisitOperand(@operator.RightOperand, @operator);
            _sql.Trim().CloseBlock.Flush();

            if (isNotNull)
            {
                _sql.Or.Flush();
                VisitOperand(@operator.LeftOperand, @operator);
                _sql.Is.Null.Trim().CloseBlock.Flush();
            }

            if (IsComparisonOperator(parent) && !IsMathOperator(@operator)) _sql.Then.True.Else.False.End.Flush();
        }
        
        private void VisitLikeExpression(Operator @operator)
        {
            _sql.OpenBlock.Trim();
            var function = @operator.LeftOperand.Projection.Function;
            var condition = (bool) @operator.RightOperand.Projection.Constant.Value;
            switch (function.Type)
            {
                case Function.FunctionType.StartsWith: 
                    _sql.StartsWith(condition,
                        x => VisitProjection(function.StartsWith.Text),
                        x => VisitProjection(function.StartsWith.Value));
                    break;
                case Function.FunctionType.Contains:
                    _sql.Contains(condition,
                        x => VisitProjection(function.Contains.Text),
                        x => VisitProjection(function.Contains.Value));
                    break;
                case Function.FunctionType.EndsWith:
                    _sql.EndsWith(condition,
                        x => VisitProjection(function.EndsWith.Text),
                        x => VisitProjection(function.EndsWith.Value));
                    break;
            }
            _sql.Trim().CloseBlock.Flush();
        }

        private void VisitOperand(Operand operand, Operator parent)
        {
            switch (operand.Type)
            {
                case Operand.OperandType.Operator: VisitOperator(operand.Operator, parent); break;
                case Operand.OperandType.Projection: VisitProjection(operand.Projection); break;
            }
        }

        private void VisitProjection(Projection projection)
        {
            var statement = ProjectionWriter<TEntity>.CreateStatement(projection, _mapping);
            _sql.Write(statement.Text);
            _parameters.AddRange(statement.Parameters);
        }

        private static void EnsureConstantIsOnRight(Operator @operator)
        {
            if (!IsLeftOperandConstant(@operator) || IsRightOperandNullConstant(@operator)) return;

            var rightOperand = @operator.RightOperand;
            @operator.RightOperand = @operator.LeftOperand;
            @operator.LeftOperand = rightOperand;
        }

        private static bool IsLikeExpression(Operand operand)
        {
            return operand.Type == Operand.OperandType.Projection &&
                   operand.Projection.Type == Projection.ProjectionType.Function &&
                   (operand.Projection.Function.Type == Function.FunctionType.StartsWith ||
                    operand.Projection.Function.Type == Function.FunctionType.Contains ||
                    operand.Projection.Function.Type == Function.FunctionType.EndsWith);
        }

        private static bool IsRightOperandNullConstant(Operator @operator)
        {
            return @operator.RightOperand.Type == Operand.OperandType.Projection &&
                   @operator.RightOperand.Projection.Type == Projection.ProjectionType.Constant &&
                   @operator.RightOperand.Projection.Constant.Value == null;
        }

        private static bool IsLeftOperandConstant(Operator @operator)
        {
            return @operator.LeftOperand.Type == Operand.OperandType.Projection &&
                   @operator.LeftOperand.Projection.Type == Projection.ProjectionType.Constant;
        }

        private static bool IsComparisonOperator(Operator @operator)
        {
            return @operator != null &&
                   (@operator.Type == Operator.OperatorType.Equal ||
                    @operator.Type == Operator.OperatorType.NotEqual ||
                    @operator.Type == Operator.OperatorType.GreaterThan ||
                    @operator.Type == Operator.OperatorType.GreaterThanOrEqual ||
                    @operator.Type == Operator.OperatorType.LessThan ||
                    @operator.Type == Operator.OperatorType.LessThanOrEqual);
        }

        private static bool IsMathOperator(Operator @operator)
        {
            return @operator != null &&
                   (@operator.Type == Operator.OperatorType.Add ||
                    @operator.Type == Operator.OperatorType.Subtract ||
                    @operator.Type == Operator.OperatorType.Multiply ||
                    @operator.Type == Operator.OperatorType.Divide ||
                    @operator.Type == Operator.OperatorType.Modulo);
        }
    }
}
