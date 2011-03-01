using System;
using System.Linq.Expressions;
using Gribble.Expressions;
using NUnit.Framework;
using Should;

namespace Tests.Expressions
{
    public static class ExpressionExtensionsTestsExtensions
    {
        public static void SomeExtensionMethod(this int value1, string value2, double value3) { }
        public static int SomeExtensionMethod(this int value1, string value2) { return 0; }
        public static int SomeExtensionMethodWithParams(this int value1, string value2, params double[] values) { return 0; }
        public static int SomeExtensionMethodWithParams(this int value1, string value2, params Expression<Func<object, object>>[] property) { return 0; }
    }

    [TestFixture]
    public class ExpressionExtensionsTests
    {
        [Test]
        public void Evaluate_Expression_Test()
        {
            Expression<Func<int>> lambda = () => 5 * 3;
            lambda.Body.EvaluateExpression().ShouldEqual(15);
        }

        [Test]
        public void First_Argument_Type_Test()
        {
            Expression<Action<int>> lambda = x => x.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            method.FirstArgumentIsOfType<int>().ShouldEqual(true);
        }

        [Test]
        public void First_Argument_Constant_Type_Test()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            method.FirstArgumentIsOfType<int>().ShouldEqual(true);
        }

        [Test]
        public void First_Argument()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            var argument = method.GetFirstArgument();
            argument.ShouldBeType(typeof (ConstantExpression));
            argument.Type.ShouldEqual(typeof(int));
            ((ConstantExpression) argument).Value.ShouldEqual(10);
        }

        [Test]
        public void Second_Argument()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            var argument = method.GetSecondArgument();
            argument.ShouldBeType(typeof(ConstantExpression));
            argument.Type.ShouldEqual(typeof(string));
            ((ConstantExpression)argument).Value.ShouldEqual("oh hai");
        }

        [Test]
        public void Third_Argument()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            var argument = method.GetThirdArgument();
            argument.ShouldBeType(typeof(ConstantExpression));
            argument.Type.ShouldEqual(typeof(double));
            ((ConstantExpression)argument).Value.ShouldEqual(5.29);
        }

        [Test]
        public void First_Constant_Argument()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            method.GetFirstConstantArgument<int>().ShouldEqual(10);
        }

        [Test]
        public void Second_Constant_Argument()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            method.GetSecondConstantArgument<string>().ShouldEqual("oh hai");
        }

        [Test]
        public void Third_Constant_Argument()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            method.GetThirdConstantArgument<double>().ShouldEqual(5.29);
        }

        [Test]
        public void Matches_Property()
        {
            Expression<Func<int>> lambda = () => "yada".Length;
            var method = lambda.Body as MemberExpression;
            method.MatchesProperty<string, int>(x => x.Length).ShouldEqual(true);
        }

        [Test]
        public void Matches_Method_No_Return()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai");
            var method = lambda.Body as MethodCallExpression;
            method.MatchesMethodSignature<int>(x => x.SomeExtensionMethod(string.Empty)).ShouldEqual(true);
        }

        [Test]
        public void Does_Not_Match_Method_No_Return()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai");
            var method = lambda.Body as MethodCallExpression;
            method.MatchesMethodSignature<int>(x => x.SomeExtensionMethod(string.Empty, 5.29)).ShouldEqual(false);
        }

        [Test]
        public void Matches_Method()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            method.MatchesMethodSignature<int>(x => x.SomeExtensionMethod(string.Empty, 5.29)).ShouldEqual(true);
        }
        
        [Test]
        public void Matches_Param_Array_Method()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethodWithParams("oh hai", 5.29, 8.99, 3.23);
            var method = lambda.Body as MethodCallExpression;
            method.MatchesMethodSignature<int>(x => x.SomeExtensionMethodWithParams(string.Empty, new double[] { })).ShouldEqual(true);
        }

        [Test]
        public void Matches_Expression_Param_Array_Method()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethodWithParams("oh hai", x => x, y => y, z => z);
            var method = lambda.Body as MethodCallExpression;
            method.MatchesMethodSignature<int>(x => x.SomeExtensionMethodWithParams(string.Empty, new Expression<Func<object, object>>[] {})).ShouldEqual(true);
        }

        [Test]
        public void Does_Not_Match_Method()
        {
            Expression<Action> lambda = () => 10.SomeExtensionMethod("oh hai", 5.29);
            var method = lambda.Body as MethodCallExpression;
            method.MatchesMethodSignature<int>(x => x.SomeExtensionMethod(string.Empty)).ShouldEqual(false);
        }
    }
}
