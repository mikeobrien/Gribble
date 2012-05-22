using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gribble.Expressions
{
    public static class ExpressionExtensions
    {
        public static T EvaluateExpression<T>(this Expression expression)
        {
            return (T)EvaluateExpression(expression);
        }

        public static object EvaluateExpression(this Expression expression)
        {
            var lambda = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)));
            var compiledExpression = lambda.Compile();
            return compiledExpression();
        }

        public static bool ArgumentIsOfType<T>(this MethodCallExpression method, int index)
        {
            return TypesAreAssignable(method.GetArgument(index).Type, typeof(T));
        }

        public static bool HasArguments(this MethodCallExpression method, int index)
        {
            return method.Arguments.Count == index;
        }

        public static Expression GetArgument(this MethodCallExpression method, int index)
        {
            return method.Arguments[index - 1];
        }

        public static T GetArgument<T>(this MethodCallExpression method, int index) where T : Expression
        {
            return (T)method.GetArgument(index);
        }

        public static T GetConstantArgument<T>(this MethodCallExpression method, int index)
        {
            return (T)((ConstantExpression)method.Arguments[index - 1]).Value;
        }

        public static bool MatchesProperty<T, TResult>(this MemberExpression member, Expression<Func<T, TResult>> expression)
        {
            var compareMember = expression.Body.StripConversion() as MemberExpression;
            if (compareMember == null) throw new Exception("Expression must be a property access.");
            return (compareMember.Member.MemberType == MemberTypes.Property &&
                    member.Member.MemberType == MemberTypes.Property &&
                    member.Member.Name == compareMember.Member.Name &&
                    ((PropertyInfo)member.Member).PropertyType == ((PropertyInfo)compareMember.Member).PropertyType);
        }

        public static bool MatchesMethodName(this MethodCallExpression method, string name)
        {
            return (method.Method.Name == name);
        }

        public static bool MatchesMethodSignature<T>(this MethodCallExpression method, Expression<Action<T>> expression)
        {
            return MatchesMethodSignatureExpression(method, expression);
        }

        public static bool MatchesMethodSignature<T>(this MethodCallExpression method, Expression<Func<T, object>> expression)
        {
            return MatchesMethodSignatureExpression(method, expression);
        }

        private static bool MatchesMethodSignatureExpression(this MethodCallExpression method, LambdaExpression expression)
        {
            var compareMethod = expression.Body.StripConversion() as MethodCallExpression;
            if (compareMethod == null) throw new Exception("Expression must be a method call.");
            return (method.Method.Name == compareMethod.Method.Name &&
                    method.Arguments.Count == compareMethod.Arguments.Count &&
                    !method.Arguments.Where((t, index) => !t.Type.TypesAreAssignable(compareMethod.Arguments[index].Type)).Any() &&
                    method.Method.ReturnType == compareMethod.Method.ReturnType);
        }

        private static bool TypesAreAssignable(this Type leftType, Type rightType)
        {
            return (leftType == rightType) ||
                   (leftType.IsGenericType && rightType.IsGenericType && 
                    leftType.GetGenericTypeDefinition() == typeof(Expression<>) && 
                    rightType.GetGenericTypeDefinition() == typeof(Expression<>)) ||
                   (leftType.IsInterface && leftType.IsAssignableFrom(rightType)) ||
                   (rightType.IsInterface && rightType.IsAssignableFrom(leftType));
        }

        private static Expression StripConversion(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert) expression = ((UnaryExpression)expression).Operand;
            return expression;
        }
    }
}
