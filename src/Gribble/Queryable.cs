﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gribble.Mapping;

namespace Gribble
{
    public class Queryable<TEntity> : IOrderedQueryable<TEntity>, INamedQueryable
    {
        private readonly IEntityMapping _mapping;
        private readonly Operations _operations;

        public Queryable(string name, IEntityMapping mapping, Operations operations, Expression expression = null)
        {
            Name = name;
            _mapping = mapping;
            _operations = operations;
            Expression = expression ?? Expression.Constant(this);
        }
        
        public string Name { get; }
        public Expression Expression { get; set; }
        public Type ElementType => typeof(TEntity);
        public IQueryProvider Provider => CreateProvider();

        public IEnumerator<TEntity> GetEnumerator() => CreateProvider()
            .Execute<IEnumerable<TEntity>>(Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private QueryProvider<TEntity> CreateProvider()
        {
            return new QueryProvider<TEntity>(Name, _mapping, _operations);
        }
    }
}
