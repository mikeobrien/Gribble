using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;

namespace Gribble
{
    public class TableFactory : ITableFactory
    {
        private readonly EntityMappingCollection _mappingCollection;
        private readonly IConnectionManager _connectionManager;

        public TableFactory(IConnectionManager connectionManagerManager, EntityMappingCollection mappingCollection)
        {
            _mappingCollection = mappingCollection;
            _connectionManager = connectionManagerManager;
        }

        public ITable<TEntity> CreateFor<TEntity>(string tableName, IProfiler profiler = null, bool noLock = false) where TEntity : class, new()
        {
            return new Table<TEntity>(
                _connectionManager,
                tableName,
                _mappingCollection.GetEntityMapping<TEntity>(),
                profiler,
                noLock);
        }

        public ITable<TEntity> CreateFor<TEntity>(string tableName, IEnumerable<ColumnMapping> mapping, IProfiler profiler = null, bool noLock = false) where TEntity : class, new()
        {
            return new Table<TEntity>(
                _connectionManager, 
                tableName, 
                _mappingCollection.GetEntityMapping<TEntity>(mapping.Select(x => new ColumnMapping(x.ColumnName, x.Name))), 
                profiler, 
                noLock);
        }
    }
}