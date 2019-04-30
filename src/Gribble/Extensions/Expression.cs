using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gribble.Extensions
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
            return TypesAreAssignable(method.ArgumentAt(index).Type, typeof(T));
        }

        public static bool HasArguments(this MethodCallExpression method, int index)
        {
            return method.Arguments.Count >= index;
        }

        public static Expression ArgumentAt(this MethodCallExpression method, int index)
        {
            return method.Arguments[index - 1];
        }

        public static T ArgumentAt<T>(this MethodCallExpression method, int index) where T : Expression
        {
            return (T)method.ArgumentAt(index);
        }

        public static T ConstantArgumentAt<T>(this MethodCallExpression method, int index)
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

        public static bool MatchesMethodSignature<T>(this MethodCallExpression method, 
            Expression<Action<T>> expression, bool compareArgumentTypes = true)
        {
            return MatchesMethodSignatureExpression(method, expression, compareArgumentTypes);
        }

        public static bool MatchesMethodSignature<T>(this MethodCallExpression method, 
            Expression<Func<T, object>> expression, bool compareArgumentTypes = true)
        {
            return MatchesMethodSignatureExpression(method, expression, compareArgumentTypes);
        }

        private static bool MatchesMethodSignatureExpression(this MethodCallExpression method, 
            LambdaExpression expression, bool compareArgumentTypes = true)
        {
            var compareMethod = expression.Body.StripConversion() as MethodCallExpression;
            if (compareMethod == null) throw new Exception("Expression must be a method call.");
            return (method.Method.Name == compareMethod.Method.Name &&
                    method.Arguments.Count == compareMethod.Arguments.Count &&
                    (!compareArgumentTypes || !method.Arguments.Where((t, index) => 
                         !t.Type.TypesAreAssignable(compareMethod.Arguments[index].Type)).Any()) &&
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

        public static PropertyInfo GetPropertyInfo(this Expression expression)
        {
            return (PropertyInfo)((MemberExpression)expression.GetLambdaBody()).Member;
        }

        public static Expression GetLambdaBody(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert ||
                   expression.NodeType == ExpressionType.Quote ||
                   expression.NodeType == ExpressionType.Lambda)  
                   expression = expression.NodeType == ExpressionType.Lambda ?
                                   ((LambdaExpression)expression).Body : 
                                   ((UnaryExpression)expression).Operand;
            return expression;
        }

        public static Expression StripConversion(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert) expression = ((UnaryExpression)expression).Operand;
            return expression;
        }

        public static PropertyInfo GetProperty<TObject, TProperty>(this Expression<Func<TObject, TProperty>> property)
        {
            if (property.Body.NodeType != ExpressionType.MemberAccess)
                throw new ArgumentException("Mapping must be a property.", nameof(property));
            return (PropertyInfo)((MemberExpression)property.Body).Member;
        }
    }
}
