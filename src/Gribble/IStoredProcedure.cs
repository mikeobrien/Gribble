using System.Collections.Generic;

namespace Gribble
{
    public interface IStoredProcedure
    {
        int Execute(string name, object parameters = null);
        TReturn Execute<TReturn>(string name, object parameters = null);
        T ExecuteScalar<T>(string name, object parameters = null);
        TEntity ExecuteSingle<TEntity>(string name, object parameters = null);
        TEntity ExecuteSingleOrNone<TEntity>(string name, object parameters = null);
        IEnumerable<TEntity> ExecuteMany<TEntity>(string name, object parameters = null);
    }
}
