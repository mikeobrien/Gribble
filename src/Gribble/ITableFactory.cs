using System.Collections.Generic;
using Gribble.Mapping;

namespace Gribble
{
    public interface ITableFactory
    {
        ITable<TEntity> CreateFor<TEntity>(string tableName, IProfiler profiler = null, bool noLock = false) 
            where TEntity : class, new();
        ITable<TEntity> CreateFor<TEntity>(string tableName, IEnumerable<ColumnMapping> mapping, IProfiler profiler = null, bool noLock = false) 
            where TEntity : class, new();
    }
}