using System.Collections.Generic;

namespace Gribble
{
    public interface IStoredProcedure
    {
        void Execute(string name);
        void Execute(string name, Dictionary<string, object> parameters);
        T ExecuteScalar<T>(string name);
        T ExecuteScalar<T>(string name, Dictionary<string, object> parameters);
        TEntity ExecuteSingle<TEntity>(string name);
        TEntity ExecuteSingle<TEntity>(string name, Dictionary<string, object> parameters);
        TEntity ExecuteSingleOrNone<TEntity>(string name);
        TEntity ExecuteSingleOrNone<TEntity>(string name, Dictionary<string, object> parameters);
        IEnumerable<TEntity> ExecuteMany<TEntity>(string name);
        IEnumerable<TEntity> ExecuteMany<TEntity>(string name, Dictionary<string, object> parameters);
    }
}
