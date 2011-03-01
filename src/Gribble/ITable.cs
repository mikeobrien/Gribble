using System;
using System.Linq;
using System.Linq.Expressions;

namespace Gribble
{
    public interface ITable<TEntity>: IOrderedQueryable<TEntity>, INamedQueryable
    {
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        void Delete(Expression<Func<TEntity, bool>> filter);
        void DeleteMany(Expression<Func<TEntity, bool>> filter);
    }
}