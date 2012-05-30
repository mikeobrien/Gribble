using System;
using System.Linq;
using System.Linq.Expressions;

namespace Gribble
{
    public interface ITable<TEntity>: IOrderedQueryable<TEntity>, INamedQueryable
    {
        TEntity Get<T>(T id);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete<T>(T id);
        void Delete(TEntity entity);
        void Delete(Expression<Func<TEntity, bool>> filter);
        int DeleteMany(Expression<Func<TEntity, bool>> filter);
        int DeleteMany(IQueryable<TEntity> source);
    }
}