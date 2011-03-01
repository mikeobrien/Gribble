using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gribble
{
    public abstract class QueryableBase<T> : IOrderedQueryable<T>, IQueryProvider
    {
        protected QueryableBase() { Expression = Expression.Constant(this); }

        public abstract QueryableBase<T> CreateQuery();
        public abstract TResult ExecuteQuery<TResult>(Expression expression);

        // ---------------------- IOrderedQueryable Implementation -----------------

        public Expression Expression { get; set; }
        public Type ElementType { get { return typeof(T); } }
        public IQueryProvider Provider { get { return this; } }

        public IEnumerator<T> GetEnumerator()
        { return Execute<IEnumerable<T>>(Expression).GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        // ---------------------- IQueryProvider Implementation -----------------

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var query = CreateQuery();
            query.Expression = expression;
            return (IQueryable<TElement>)query;
        }

        public IQueryable CreateQuery(Expression expression) { return CreateQuery<T>(expression); }
        public TResult Execute<TResult>(Expression expression) { return ExecuteQuery<TResult>(expression); }
        public object Execute(Expression expression) { return Execute<object>(expression); }
    }
}
