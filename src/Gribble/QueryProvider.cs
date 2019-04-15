﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Gribble.Expressions;
using Gribble.Model;

namespace Gribble
{
    public class QueryProvider<TEntity> : IQueryProvider 
    {
        private readonly string _name;
        private readonly Operations _operations;

        public QueryProvider(string name, Operations operations)
        {
            _name = name;
            _operations = operations;
        }

        public IQueryable CreateQuery(Expression expression) => CreateQuery<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new Queryable<TElement>(_name, _operations, expression);
        }

        public object Execute(Expression expression) { return Execute<object>(expression); }

        public TResult Execute<TResult>(Expression expression)
        {
            var query = QueryVisitor<TEntity>.CreateModel(expression, x => ((INamedQueryable) x).Name);
            switch (query.Operation) {
                case Query.OperationType.Query:
                    if (query.Select.From.IsTable)
                    {
                        if (query.Select.From.Table == null)
                            query.Select.From.Table = new Table();
                        if (query.Select.From.Table.Name == null)
                            query.Select.From.Table.Name = _name;
                    }
                    return _operations.ExecuteQuery<TEntity, TResult>(query.Select);
                case Query.OperationType.CopyTo: return (TResult)_operations.CopyInto<TEntity>(query.CopyTo);
                case Query.OperationType.SyncWith: return (TResult)_operations.SyncWith<TEntity>(query.SyncWith);
                default: throw new NotImplementedException();
            }
        }
    }
}
