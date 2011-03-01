using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gribble.Expressions
{
    public abstract class ExpressionVisitorBase<TState>
    {
        public class ExpressionNotSupportedException : Exception
        {
            public ExpressionNotSupportedException(Expression expression) :
                base(string.Format("Expression {0} not supported.", expression.GetType().Name)) { }
        }

        public virtual TState Visit(Expression node)
        {
            return Visit(node, default(TState));
        }

        public virtual TState Visit(Expression node, TState state)
        {
            var context = Context.Create(state);
            Visit(context, node);
            return context.State;
        }

        protected void Visit(Context context, Expression node)
        {
            ValidateArguments(context, node);

            if (node is BinaryExpression) VisitBinary(context, (BinaryExpression)node);
            else if (node is BlockExpression) VisitBlock(context, (BlockExpression)node);
            else if (node is ConditionalExpression) VisitConditional(context, (ConditionalExpression)node);
            else if (node is ConstantExpression) VisitConstant(context, (ConstantExpression)node);
            else if (node is DebugInfoExpression) VisitDebugInfo(context, (DebugInfoExpression)node);
            else if (node is DefaultExpression) VisitDefault(context, (DefaultExpression)node);
            else if (node is DynamicExpression) VisitDynamic(context, (DynamicExpression)node);
            else if (node is GotoExpression) VisitGoto(context, (GotoExpression)node);
            else if (node is IndexExpression) VisitIndex(context, (IndexExpression)node);
            else if (node is InvocationExpression) VisitInvocation(context, (InvocationExpression)node);
            else if (node is LabelExpression) VisitLabel(context, (LabelExpression)node);
            else if (node is LambdaExpression) VisitLambda(context, (LambdaExpression)node);
            else if (node is ListInitExpression) VisitListInit(context, (ListInitExpression)node);
            else if (node is LoopExpression) VisitLoop(context, (LoopExpression)node);
            else if (node is MemberExpression) VisitMember(context, (MemberExpression)node);
            else if (node is MemberInitExpression) VisitMemberInit(context, (MemberInitExpression)node);
            else if (node is MethodCallExpression) VisitMethodCall(context, (MethodCallExpression)node);
            else if (node is NewArrayExpression) VisitNewArray(context, (NewArrayExpression)node);
            else if (node is NewExpression) VisitNew(context, (NewExpression)node);
            else if (node is ParameterExpression) VisitParameter(context, (ParameterExpression)node);
            else if (node is RuntimeVariablesExpression) VisitRuntimeVariables(context, (RuntimeVariablesExpression)node);
            else if (node is SwitchExpression) VisitSwitch(context, (SwitchExpression)node);
            else if (node is TryExpression) VisitTry(context, (TryExpression)node);
            else if (node is TypeBinaryExpression) VisitTypeBinary(context, (TypeBinaryExpression)node);
            else if (node is UnaryExpression) VisitUnary(context, (UnaryExpression)node);
            else throw new ExpressionNotSupportedException(node);
        }

        private void Visit(Context context, IEnumerable<Expression> nodes)
        {
            foreach (var node in nodes) Visit(context, node);
        }

        private void Visit(IDictionary<Expression, Context> nodes)
        {
            foreach (var node in nodes) Visit(node.Value, node.Key);
        }

        private static void Visit<T>(Context context, IEnumerable<T> nodes, Action<Context, T> elementVisitor)
        {
            foreach (var node in nodes) elementVisitor(context, node);
        }

        private static void ValidateArguments(object node)
        {
            if (node == null) throw new ArgumentNullException("node");
        }

        private static void ValidateArguments(Context context, object node)
        {
            if (node == null) throw new ArgumentNullException("node");
            if (context == null) throw new ArgumentNullException("context");
        }

        protected virtual void VisitBinary(Context context, BinaryExpression node)
        {
            ValidateArguments(context, node);
            VisitBinary(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitBinary(BinaryExpression node, TState leftState, TState conversionState, TState rightState, 
                                    bool visitLeft, bool visitConversion, bool visitRight)
        {
            ValidateArguments(node);
            if (visitLeft && node.Left != null) Visit(Context.Create(node, leftState), node.Left);
            if (visitConversion && node.Conversion != null) Visit(Context.Create(node, conversionState), node.Conversion);
            if (visitRight && node.Right != null) Visit(Context.Create(node, rightState), node.Right);
        }

        protected virtual void VisitBlock(Context context, BlockExpression node)
        {
            ValidateArguments(context, node);
            VisitBlock(node, context.State, context.State, true, true, System.Linq.Enumerable.Empty<Expression>(), System.Linq.Enumerable.Empty<ParameterExpression>());
        }

        protected void VisitBlock(BlockExpression node, TState expressionsState, TState variablesState, 
                                    bool visitExpressions, bool visitVariables, 
                                    IEnumerable<Expression> expressionsNotToVisit, 
                                    IEnumerable<ParameterExpression> variablesNotToVisit)
        {
            ValidateArguments(node);
            if (visitExpressions && node.Expressions != null) Visit(Context.Create(node, expressionsState), node.Expressions.Except(expressionsNotToVisit));
            if (visitVariables && node .Variables!= null) Visit(Context.Create(node, variablesState), node.Variables.Except(variablesNotToVisit));
        }

        protected virtual void VisitCatchBlock(Context context, CatchBlock node)
        {
            ValidateArguments(context, node);
            VisitCatchBlock(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitCatchBlock(CatchBlock node, TState variableState, TState filterState, TState bodyState, bool visitVariable, bool visitFilter, bool visitBody)
        {
            ValidateArguments(node);
            if (visitVariable && node.Variable != null) Visit(Context.Create(node, variableState), node.Variable);
            if (visitFilter && node.Filter != null) Visit(Context.Create(node, filterState), node.Filter);
            if (visitBody && node.Body != null) Visit(Context.Create(node, bodyState), node.Body);
        }

        protected virtual void VisitConditional(Context context, ConditionalExpression node)
        {
            ValidateArguments(context, node);
            VisitConditional(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitConditional(ConditionalExpression node, TState testState, TState trueState, TState falseState, bool visitTest, bool visitTrue, bool visitFalse)
        {
            ValidateArguments(node);
            if (visitTest && node.Test != null) Visit(Context.Create(node, testState), node.Test);
            if (visitTrue && node.IfTrue != null) Visit(Context.Create(node, trueState), node.IfTrue);
            if (visitFalse && node.IfFalse != null) Visit(Context.Create(node, falseState), node.IfFalse);
        }

        protected virtual void VisitConstant(Context context, ConstantExpression node) { }
        protected virtual void VisitDebugInfo(Context context, DebugInfoExpression node) { }
        protected virtual void VisitDefault(Context context, DefaultExpression node) { }

        protected virtual void VisitDynamic(Context context, DynamicExpression node)
        {
            ValidateArguments(context, node);
            VisitDynamic(node, context.State);
        }

        protected void VisitDynamic(DynamicExpression node, TState state, params Expression[] argsNotToVisit)
        {
            ValidateArguments(node);
            if (node.Arguments != null) Visit(Context.Create(node, state), node.Arguments.Except(argsNotToVisit), Visit);
        }

        protected virtual void VisitElementInit(Context context, ElementInit node)
        {
            ValidateArguments(context, node);
            VisitElementInit(node, context.State);
        }

        protected void VisitElementInit(ElementInit node, TState state, params Expression[] argsNotToVisit)
        {
            ValidateArguments(node);
            if (node.Arguments != null) Visit(Context.Create(node, state), node.Arguments.Except(argsNotToVisit), Visit);
        }

        protected virtual void VisitGoto(Context context, GotoExpression node)
        {
            ValidateArguments(context, node);
            VisitGoto(node, context.State, context.State, true, true);
        }

        protected void VisitGoto(GotoExpression node, TState targetState, TState valueState, bool visitTarget, bool visitValue)
        {
            ValidateArguments(node);
            if (visitTarget && node.Target != null) VisitLabelTarget(Context.Create(node, targetState), node.Target);
            if (visitValue && node.Value != null) Visit(Context.Create(node, valueState), node.Value);
        }

        protected virtual void VisitIndex(Context context, IndexExpression node)
        {
            ValidateArguments(context, node);
            VisitIndex(node, context.State, context.State, true, true);
        }

        protected void VisitIndex(IndexExpression node, TState objectState, TState argumentsState, bool visitObject, bool visitArguments, params Expression[] argsNotToVisit)
        {
            ValidateArguments(node);
            if (visitObject && node.Object != null) Visit(Context.Create(node, objectState), node.Object);
            if (visitArguments && node.Arguments != null) Visit(Context.Create(node, argumentsState), node.Arguments.Except(argsNotToVisit), Visit);
        }

        protected virtual void VisitInvocation(Context context, InvocationExpression node)
        {
            ValidateArguments(context, node);
            VisitInvocation(node, context.State, context.State, true, true);
        }

        protected void VisitInvocation(InvocationExpression node, TState expressionState, TState argumentsState, bool visitExpression, bool visitArguments, params Expression[] argsNotToVisit)
        {
            ValidateArguments(node);
            if (visitExpression && node.Expression != null) Visit(Context.Create(node, expressionState), node.Expression);
            if (visitArguments && node.Arguments != null) Visit(Context.Create(node, argumentsState), node.Arguments.Except(argsNotToVisit), Visit);
        }

        protected virtual void VisitLabel(Context context, LabelExpression node)
        {
            ValidateArguments(context, node);
            VisitLabel(node, context.State, context.State, true, true);
        }

        protected void VisitLabel(LabelExpression node, TState targetState, TState defaultValueState, bool visitTarget, bool visitDefaultValue)
        {
            ValidateArguments(node);
            if (visitTarget && node.Target != null) VisitLabelTarget(Context.Create(node, targetState), node.Target);
            if (visitDefaultValue && node.DefaultValue != null) Visit(Context.Create(node, defaultValueState), node.DefaultValue);
        }

        protected virtual void VisitLabelTarget(Context context, LabelTarget node) { }

        protected virtual void VisitLambda(Context context, LambdaExpression node)
        {
            ValidateArguments(context, node);
            VisitLambda(node, context.State, context.State, true, true);
        }

        protected void VisitLambda(LambdaExpression node, TState bodyState, TState parametersState, bool visitBody, bool visitParameters, params ParameterExpression[] paramsNotToVisit)
        {
            ValidateArguments(node);
            if (visitBody && node.Body != null) Visit(Context.Create(node, bodyState), node.Body);
            if (visitParameters && node.Parameters != null) Visit(Context.Create(node, parametersState), node.Parameters.Except(paramsNotToVisit), Visit);
        }

        protected virtual void VisitListInit(Context context, ListInitExpression node)
        {
            ValidateArguments(context, node);
            VisitListInit(node, context.State, context.State, true, true);
        }

        protected void VisitListInit(ListInitExpression node, TState newExpressionState, TState initializerState, bool visitNewExpression, bool visitInitializers, params ElementInit[] initializersNotToVisit)
        {
            ValidateArguments(node);
            if (visitNewExpression && node.NewExpression != null) Visit(Context.Create(node, newExpressionState), node.NewExpression);
            if (visitInitializers && node.Initializers != null) Visit(Context.Create(node, initializerState), node.Initializers.Except(initializersNotToVisit), VisitElementInit);
        }

        protected virtual void VisitLoop(Context context, LoopExpression node)
        {
            ValidateArguments(context, node);
            VisitLoop(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitLoop(LoopExpression node, TState breakLabelState, TState continueLabelState, TState bodyState, bool visitBreakLabel, bool visitContinueLabel, bool visitBody)
        {
            ValidateArguments(node);
            if (visitBreakLabel && node.BreakLabel != null) VisitLabelTarget(Context.Create(node, breakLabelState), node.BreakLabel);
            if (visitContinueLabel && node.ContinueLabel != null) VisitLabelTarget(Context.Create(node, continueLabelState), node.ContinueLabel);
            if (visitBody && node.Body != null) Visit(Context.Create(node, bodyState), node.Body);
        }

        protected virtual void VisitMember(Context context, MemberExpression node)
        {
            ValidateArguments(context, node);
            VisitMember(node, context.State);
        }

        protected void VisitMember(MemberExpression node, TState state)
        {
            ValidateArguments(node);
            if (node.Expression != null) Visit(Context.Create(node, state), node.Expression);
        }

        protected virtual void VisitMemberAssignment(Context context, MemberAssignment node)
        {
            ValidateArguments(context, node);
            VisitMemberAssignment(node, context.State);
        }

        protected void VisitMemberAssignment(MemberAssignment node, TState state)
        {
            ValidateArguments(node);
            if (node.Expression != null) Visit(Context.Create(node, state), node.Expression);
        }

        protected virtual void VisitMemberBinding(Context context, MemberBinding node)
        {
            ValidateArguments(context, node);
            VisitMemberBinding(context, node, context.State);
        }

        protected void VisitMemberBinding(Context context, MemberBinding node, TState state)
        {
            ValidateArguments(node);
            switch (node.BindingType)
            {
                case MemberBindingType.Assignment: VisitMemberAssignment(Context.Create(context.Parent, state), (MemberAssignment)node); break;
                case MemberBindingType.MemberBinding: VisitMemberMemberBinding(Context.Create(context.Parent, state), (MemberMemberBinding)node); break;
                case MemberBindingType.ListBinding: VisitMemberListBinding(Context.Create(context.Parent, state), (MemberListBinding)node); break;
            }
            throw new Exception("Cannont bind type " + node.BindingType);
        }

        protected virtual void VisitMemberInit(Context context, MemberInitExpression node)
        {
            ValidateArguments(context, node);
            VisitMemberInit(node, context.State, context.State, true, true);
        }

        protected void VisitMemberInit(MemberInitExpression node, TState newExpressionState, TState bindingsState, bool visitNewExpression, bool visitBindings, params MemberBinding[] bindingsNotToVisit)
        {
            ValidateArguments(node);
            if (visitNewExpression && node.NewExpression != null) Visit(Context.Create(node, newExpressionState), node.NewExpression);
            if (visitBindings && node.Bindings != null) Visit(Context.Create(node, bindingsState), node.Bindings.Except(bindingsNotToVisit), VisitMemberBinding);
        }

        protected virtual void VisitMemberListBinding(Context context, MemberListBinding node)
        {
            ValidateArguments(context, node);
            VisitMemberListBinding(node, context.State);
        }

        protected void VisitMemberListBinding(MemberListBinding node, TState state, params ElementInit[] initializersNotToVisit)
        {
            ValidateArguments(node);
            if (node.Initializers != null) Visit(Context.Create(node, state), node.Initializers.Except(initializersNotToVisit), VisitElementInit);
        }

        protected virtual void VisitMemberMemberBinding(Context context, MemberMemberBinding node)
        {
            ValidateArguments(context, node);
            VisitMemberMemberBinding(node, context.State);
        }

        protected void VisitMemberMemberBinding(MemberMemberBinding node, TState state, params MemberBinding[] memberBindingsNotToVisit)
        {
            ValidateArguments(node);
            if (node.Bindings != null) Visit(Context.Create(node, state), node.Bindings.Except(memberBindingsNotToVisit), VisitMemberBinding);
        }

        protected virtual void VisitMethodCall(Context context, MethodCallExpression node)
        {
            ValidateArguments(context, node);
            VisitMethodCall(node, context.State, context.State, true, true);
        }

        protected void VisitMethodCall(MethodCallExpression node, TState objectState, bool visitObject)
        {
            VisitMethodCall(node, objectState, null, visitObject, false);
        }

        protected void VisitMethodCall(MethodCallExpression node, IDictionary<Expression, TState> argumentsState, bool visitArguments, params Expression[] argumentsNotToVisit)
        {
            VisitMethodCall(node, default(TState), argumentsState, false, visitArguments, argumentsNotToVisit);
        }

        protected void VisitMethodCall(MethodCallExpression node, TState objectState, TState argumentsState, bool visitObject, bool visitArguments, params Expression[] argumentsNotToVisit)
        {
            VisitMethodCall(node, objectState, node.Arguments.ToDictionary(x => x, x => objectState), visitObject, visitArguments, argumentsNotToVisit);
        }

        protected void VisitMethodCall(MethodCallExpression node, TState objectState, IDictionary<Expression, TState> argumentsState, bool visitObject, bool visitArguments, params Expression[] argumentsNotToVisit)
        {
            ValidateArguments(node);
            if (visitObject && node.Object != null) Visit(Context.Create(node, objectState), node.Object);
            if (visitArguments && node.Arguments != null) Visit(argumentsState.Where(x => !argumentsNotToVisit.Contains(x.Key)).ToDictionary(x => x.Key, x => Context.Create(x.Key, x.Value)));
        }

        protected virtual void VisitNew(Context context, NewExpression node)
        {
            ValidateArguments(context, node);
            VisitNew(node, context.State);
        }

        protected void VisitNew(NewExpression node, TState state, params Expression[] argumentsNotToVisit)
        {
            ValidateArguments(node);
            if (node.Arguments != null) Visit(Context.Create(node, state), node.Arguments.Except(argumentsNotToVisit));
        }

        protected virtual void VisitNewArray(Context context, NewArrayExpression node)
        {
            ValidateArguments(context, node);
            VisitNewArray(node, context.State);
        }

        protected void VisitNewArray(NewArrayExpression node, TState state, params Expression[] expressionsNotToVisit)
        {
            ValidateArguments(node);
            if (node.Expressions != null) Visit(Context.Create(node, state), node.Expressions.Except(expressionsNotToVisit));
        }

        protected virtual void VisitParameter(Context context, ParameterExpression node) { }

        protected virtual void VisitRuntimeVariables(Context context, RuntimeVariablesExpression node)
        {
            ValidateArguments(context, node);
            VisitRuntimeVariables(node, context.State);
        }

        protected void VisitRuntimeVariables(RuntimeVariablesExpression node, TState state, params ParameterExpression[] variablesNotToVisit)
        {
            ValidateArguments(node);
            if (node.Variables != null) Visit(Context.Create(node, state), node.Variables.Except(variablesNotToVisit), Visit);
        }

        protected virtual void VisitSwitch(Context context, SwitchExpression node)
        {
            ValidateArguments(context, node);
            VisitSwitch(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitSwitch(SwitchExpression node, TState switchValueState, TState switchCasesState, TState defaultBodyState, bool visitSwitchValue, bool visitSwitchCases, bool visitDefaultBody, params SwitchCase[] switchCasesNotToVisit)
        {
            ValidateArguments(node);
            if (visitSwitchValue && node.SwitchValue != null) Visit(Context.Create(node, switchValueState), node.SwitchValue);
            if (visitSwitchCases && node.Cases != null) Visit(Context.Create(node, switchCasesState), node.Cases.Except(switchCasesNotToVisit), VisitSwitchCase);
            if (visitDefaultBody && node.DefaultBody != null) Visit(Context.Create(node, defaultBodyState), node.DefaultBody);
        }

        protected virtual void VisitSwitchCase(Context context, SwitchCase node)
        {
            ValidateArguments(context, node);
            VisitSwitchCase(node, context.State, context.State, true, true);
        }

        protected void VisitSwitchCase(SwitchCase node, TState testValuesState, TState bodyState, bool visitTestValues, bool visitBody, params Expression[] testValuesNotToVisit)
        {
            ValidateArguments(node);
            if (visitTestValues && node.TestValues != null) Visit(Context.Create(node, testValuesState), node.TestValues.Except(testValuesNotToVisit));
            if (visitBody && node.Body != null) Visit(Context.Create(node, bodyState), node.Body);
        }

        protected virtual void VisitTry(Context context, TryExpression node)
        {
            ValidateArguments(context, node);
            VisitTry(node, context.State, context.State, context.State, context.State, true, true, true, true);
        }

        protected void VisitTry(TryExpression node, TState bodyState, TState handlersState, TState finallyState, TState faultState, bool visitBody, bool visitHandlers, bool visitFinally, bool visitFault, params CatchBlock[] catchBlocksNotToVisit)
        {
            ValidateArguments(node);
            if (visitBody && node.Body != null) Visit(Context.Create(node, bodyState), node.Body);
            if (visitHandlers && node.Handlers != null) Visit(Context.Create(node, handlersState), node.Handlers.Except(catchBlocksNotToVisit), VisitCatchBlock);
            if (visitFinally && node.Finally != null) Visit(Context.Create(node, finallyState), node.Finally);
            if (visitFault && node.Fault != null) Visit(Context.Create(node, faultState), node.Fault);
        }

        protected virtual void VisitTypeBinary(Context context, TypeBinaryExpression node)
        {
            ValidateArguments(context, node);
            VisitTypeBinary(node, context.State);
        }

        protected void VisitTypeBinary(TypeBinaryExpression node, TState state)
        {
            ValidateArguments(node);
            if (node.Expression != null) Visit(Context.Create(node, state), node.Expression);
        }

        protected virtual void VisitUnary(Context context, UnaryExpression node)
        {
            ValidateArguments(context, node);
            VisitUnary(node, context.State);
        }

        protected void VisitUnary(UnaryExpression node, TState state)
        {
            ValidateArguments(node);
            if (node.Operand != null) Visit(Context.Create(node, state), node.Operand);
        }

        public class Context
        {
            private Context(ExpressionParent expressionParent, TState state) 
            {
                Parent = expressionParent;
                State = state;
            }

            public static Context Create(TState state) { return new Context(null, state); }

            public static Context Create(ExpressionParent parent, TState state) { return new Context(parent, state); }
            public static Context Create(Expression parentExpression, TState state) { return new Context(new ExpressionParent(parentExpression), state); }
            public static Context Create(SwitchCase parentSwitchCase, TState state) { return new Context(new ExpressionParent(parentSwitchCase), state); }
            public static Context Create(MemberBinding parentMemberBinding, TState state) { return new Context(new ExpressionParent(parentMemberBinding), state); }
            public static Context Create(MemberMemberBinding parentMemberMemberBinding, TState state) { return new Context(new ExpressionParent(parentMemberMemberBinding), state); }
            public static Context Create(MemberListBinding parentMemberListBinding, TState state) { return new Context(new ExpressionParent(parentMemberListBinding), state); }
            public static Context Create(MemberAssignment parentMemberAssignment, TState state) { return new Context(new ExpressionParent(parentMemberAssignment), state); }
            public static Context Create(ElementInit parentElementInit, TState state) { return new Context(new ExpressionParent(parentElementInit), state); }
            public static Context Create(CatchBlock parentCatchBlock, TState state) { return new Context(new ExpressionParent(parentCatchBlock), state); }

            public bool HasParent { get { return Parent != null; } }
            public ExpressionParent Parent { get; private set; }

            public TState State { get; set; }
        }

        public class ExpressionParent
        {
            public enum ParentType { Expression, SwitchCase, MemberBinding, MemberMemberBinding, MemberListBinding, MemberAssignment, ElementInit, CatchBlock }

            public ExpressionParent(Expression expression) { Expression = expression; Type = ParentType.Expression; }
            public ExpressionParent(SwitchCase switchCase) { SwitchCase = switchCase; Type = ParentType.SwitchCase; }
            public ExpressionParent(MemberBinding memberBinding) { MemberBinding = memberBinding; Type = ParentType.MemberBinding; }
            public ExpressionParent(MemberMemberBinding memberMemberBinding) { MemberMemberBinding = memberMemberBinding; Type = ParentType.MemberMemberBinding; }
            public ExpressionParent(MemberListBinding memberListBinding) { MemberListBinding = memberListBinding; Type = ParentType.MemberListBinding; }
            public ExpressionParent(MemberAssignment memberAssignment) { MemberAssignment = memberAssignment; Type = ParentType.MemberAssignment; }
            public ExpressionParent(ElementInit elementInit) { ElementInit = elementInit; Type = ParentType.ElementInit; }
            public ExpressionParent(CatchBlock catchBlock) { CatchBlock = catchBlock; Type = ParentType.CatchBlock; }

            public readonly ParentType Type;
            public readonly Expression Expression;
            public readonly SwitchCase SwitchCase;
            public readonly MemberBinding MemberBinding;
            public readonly MemberMemberBinding MemberMemberBinding;
            public readonly MemberListBinding MemberListBinding;
            public readonly MemberAssignment MemberAssignment;
            public readonly ElementInit ElementInit;
            public readonly CatchBlock CatchBlock;

            public bool IsExpressionOfType<T>()
            {
                return Type == ParentType.Expression && Expression is T;
            }

            public bool IsExpressionOf(ExpressionType type)
            {
                return Type == ParentType.Expression && Expression.NodeType == type;
            }

            public bool IsBinaryLogicalOperator()
            {
                if (Type != ParentType.Expression) return false;
                switch (Expression.NodeType)
                {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse: return true;
                    default: return false;
                }
            }

            public bool IsBinaryComparisonOperator()
            {
                if (Type != ParentType.Expression) return false;
                switch (Expression.NodeType)
                {
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.NotEqual: return true;
                    default: return false;
                }
            }

            public bool IsBinaryMathOperator()
            {
                if (Type != ParentType.Expression) return false;
                switch (Expression.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.Subtract: return true;
                    default: return false;
                }
            }
        }
    }
}
