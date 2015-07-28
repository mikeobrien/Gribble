using System.Collections.Generic;

namespace Gribble
{
    public interface ISqlStatement
    {
        int Execute(string commandText, object parameters = null);
        T ExecuteScalar<T>(string commandText, object parameters = null);
        TEntity ExecuteSingle<TEntity>(string commandText, object parameters = null) where TEntity : class;
        TEntity ExecuteSingleOrNone<TEntity>(string commandText, object parameters = null) where TEntity : class;
        IEnumerable<TEntity> ExecuteMany<TEntity>(string commandText, object parameters = null) where TEntity : class;
    }
}
