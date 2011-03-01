using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gribble.Statements;

namespace Gribble.Expressions
{
    public class ProjectionVisitor<T> : ExpressionVisitorBase<Action<Projection>>
    {
        public class MemberTypeNotSupportedException : Exception
        {
            public MemberTypeNotSupportedException(string name, MemberTypes type) :
                base(string.Format("Member type of {0} not supported. Member name: {1}.", type, name)) { }
        }

        private readonly string _tableAlias;

        public ProjectionVisitor(string tableAlias)
        {
            _tableAlias = tableAlias;
        }

        public static Projection CreateModel(Expression expression, string tableAlias = null)
        {
            Projection projection = null;
            var visitor = new ProjectionVisitor<T>(tableAlias);
            visitor.Visit(expression, x => projection = x);
            return projection;
        }

        protected override void VisitBinary(Context context, BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Coalesce:
                    var projection = Projection.Create.Function(Function.Create.Coalesce());
                    context.State(projection);
                    VisitBinary(node, 
                                x => projection.Function.Coalesce.First = x,
                                null,
                                x => projection.Function.Coalesce.Second = x,
                                true, false, true);
                    break;
                default: throw new OperatorNotSupportedException(node.NodeType);
            }
        }

        protected override void VisitUnary(Context context, UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                    var projection = Projection.Create.Function(Function.Create.Convert(node.Type));
                    context.State(projection);
                    VisitUnary(node, x => projection.Function.Convert.Value = x);
                    break;
                case ExpressionType.Quote: base.VisitUnary(context, node); break;
                default: throw new OperatorNotSupportedException(node.NodeType);
            }
        }

        protected override void VisitMethodCall(Context context, MethodCallExpression node)
        {
            if (node.MatchesMethodSignature<string>(x => x.StartsWith(string.Empty)))
            {
                var projection = Projection.Create.Function(Function.Create.StartsWith());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>>
                    { {node.GetFirstArgument(), x => projection.Function.StartsWith.Value = x} };
                VisitMethodCall(node, x => projection.Function.StartsWith.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.Contains(string.Empty)))
            {
                var projection = Projection.Create.Function(Function.Create.Contains());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.Contains.Value = x } };
                VisitMethodCall(node, x => projection.Function.Contains.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.EndsWith(string.Empty)))
            {
                var projection = Projection.Create.Function(Function.Create.EndsWith());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.EndsWith.Value = x } };
                VisitMethodCall(node, x => projection.Function.EndsWith.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.ToLower()))
            {
                var projection = Projection.Create.Function(Function.Create.ToLower());
                context.State(projection);
                VisitMethodCall(node, x => projection.Function.ToLower.Text = x, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.ToUpper()))
            {
                var projection = Projection.Create.Function(Function.Create.ToUpper());
                context.State(projection);
                VisitMethodCall(node, x => projection.Function.ToUpper.Text = x, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.Trim()))
            {
                var projection = Projection.Create.Function(Function.Create.Trim());
                context.State(projection);
                VisitMethodCall(node, x => projection.Function.Trim.Text = x, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.TrimEnd()))
            {
                var projection = Projection.Create.Function(Function.Create.TrimEnd());
                context.State(projection);
                VisitMethodCall(node, x => projection.Function.TrimEnd.Text = x, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.TrimStart()))
            {
                var projection = Projection.Create.Function(Function.Create.TrimStart());
                context.State(projection);
                VisitMethodCall(node, x => projection.Function.TrimStart.Text = x, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.ToString()))
            {
                var projection = Projection.Create.Function(Function.Create.ToString());
                context.State(projection);
                VisitMethodCall(node, x => projection.Function.ToString.Value = x, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.Substring(0)))
            {
                var projection = Projection.Create.Function(Function.Create.Substring());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.Substring.Start = x } };
                VisitMethodCall(node, x => projection.Function.Substring.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.Substring(0, 0)))
            {
                var projection = Projection.Create.Function(Function.Create.SubstringFixed());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.SubstringFixed.Start = x },
                      { node.GetSecondArgument(), x => projection.Function.SubstringFixed.Length = x }};
                VisitMethodCall(node, x => projection.Function.SubstringFixed.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.Replace(string.Empty, string.Empty)))
            {
                var projection = Projection.Create.Function(Function.Create.Replace());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.Replace.SearchValue = x },
                      { node.GetSecondArgument(), x => projection.Function.Replace.ReplaceValue = x }};
                VisitMethodCall(node, x => projection.Function.Replace.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.Insert(0, string.Empty)))
            {
                var projection = Projection.Create.Function(Function.Create.Insert());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.Insert.Start = x },
                      { node.GetSecondArgument(), x => projection.Function.Insert.Value = x }};
                VisitMethodCall(node, x => projection.Function.Insert.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.IndexOf(string.Empty)))
            {
                var projection = Projection.Create.Function(Function.Create.IndexOf());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.IndexOf.Value = x } };
                VisitMethodCall(node, x => projection.Function.IndexOf.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.IndexOf(string.Empty, 0)))
            {
                var projection = Projection.Create.Function(Function.Create.IndexOfAt());
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.IndexOfAt.Value = x },
                      { node.GetSecondArgument(), x => projection.Function.IndexOfAt.Start = x }};
                VisitMethodCall(node, x => projection.Function.IndexOfAt.Text = x, argumentsState, true, true);
            }
            else if (node.MatchesMethodSignature<string>(x => x.Hash(EntityExtensions.HashAlgorithim.Md5)))
            {
                var projection = Projection.Create.Function(Function.Create.Hash(node.GetSecondConstantArgument<EntityExtensions.HashAlgorithim>() == EntityExtensions.HashAlgorithim.Md5 ? 
                                                                                 Function.HashParameters.HashType.Md5 : 
                                                                                 Function.HashParameters.HashType.Sha1));
                context.State(projection);
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.Hash.Value = x } };
                VisitMethodCall(node, argumentsState, true, node.GetSecondArgument());
            }
            else if (node.MatchesMethodSignature<byte[]>(x => x.ToHex()))
            {
                var projection = Projection.Create.Function(Function.Create.ToHex());
                context.State(projection); 
                var argumentsState = new Dictionary<Expression, Action<Projection>> 
                    { { node.GetFirstArgument(), x => projection.Function.ToHex.Value = x }};
                VisitMethodCall(node, argumentsState, true);
            }
            else if (node.Object != null && node.Object.NodeType == ExpressionType.MemberAccess && 
                     node.Object.Type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>)) && 
                     node.MatchesMethodName("get_Item"))
                context.State(Projection.Create.Field(((MemberExpression)node.Object).Member.Name, node.Arguments[0].EvaluateExpression<string>(), _tableAlias));
            else context.State(Projection.Create.Constant(node.EvaluateExpression()));
        }

        protected override void VisitMember(Context context, MemberExpression node)
        {
            if (node.MatchesProperty<string, int>(x => x.Length))
            {
                var projection = Projection.Create.Function(Function.Create.Length());
                context.State(projection);
                VisitMember(node, x => projection.Function.Length.Text = x);
            }
            else if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter && 
                     node.Member.DeclaringType == typeof(T))
            {
                if (node.Member.MemberType != MemberTypes.Property)
                    throw new MemberTypeNotSupportedException(node.Member.Name, node.Member.MemberType);
                context.State(Projection.Create.Field(node.Member.Name, _tableAlias));
            }
            else context.State(Projection.Create.Constant(node.EvaluateExpression()));
        }

        protected override void VisitConstant(Context context, ConstantExpression node)
        {
            context.State(Projection.Create.Constant(node.Value));
        }
    }
}
