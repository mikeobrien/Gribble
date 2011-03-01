using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gribble;

namespace Tests
{
    public class MockQueryable<T> : IOrderedQueryable<T>, IQueryProvider, INamedQueryable
    {
        private readonly Action<Expression> _updateRootExpression;

        protected MockQueryable(string name)
        {
            _updateRootExpression = x => Expression = x;
            Expression = Expression.Constant(this);
            Name = name;
        }

        private MockQueryable(Expression expression, Action<Expression> updateRootExpression, string name)
        {
            _updateRootExpression = updateRootExpression;
            Expression = expression;
            Name = name;
        }

        public static MockQueryable<T> Create(string name) { return new MockQueryable<T>(name); }
        public static MockQueryable<T> Create() { return new MockQueryable<T>(string.Empty); }

        public string Name { get; private set; }

        // ---------------------- IOrderedQueryable Implementation -----------------

        public Expression Expression { get; set; }
        public Type ElementType { get { return typeof(T); } }
        public IQueryProvider Provider { get { return this; } }
        public IEnumerator<T> GetEnumerator() { return System.Linq.Enumerable.Empty<T>().GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        // ---------------------- IQueryProvider Implementation -----------------

        public IQueryable CreateQuery(Expression expression) { return CreateQuery<T>(expression); }
        public object Execute(Expression expression) { return Execute<object>(expression); }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            _updateRootExpression(expression);
            return new MockQueryable<TElement>(expression, _updateRootExpression, Name);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            _updateRootExpression(expression);
            return default(TResult);
        }
    }
}
