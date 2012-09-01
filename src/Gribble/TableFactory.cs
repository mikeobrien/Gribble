using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;

namespace Gribble
{
    public class TableFactory : ITableFactory
    {
        private readonly EntityMappingCollection _mappingCollection;
        private readonly IProfiler _profiler;
        private readonly IConnectionManager _connectionManager;

        public TableFactory(IConnectionManager connectionManagerManager, EntityMappingCollection mappingCollection, IProfiler profiler)
        {
            _mappingCollection = mappingCollection;
            _profiler = profiler;
            _connectionManager = connectionManagerManager;
        }

        public ITable<TEntity> CreateFor<TEntity>(string tableName, bool noLock = false) where TEntity : class, new()
        {
            return new Table<TEntity>(
                _connectionManager,
                tableName,
                _mappingCollection.GetEntityMapping<TEntity>(),
                _profiler,
                noLock);
        }

        public ITable<TEntity> CreateFor<TEntity>(string tableName, IEnumerable<ColumnMapping> mapping, bool noLock = false) where TEntity : class, new()
        {
            return new Table<TEntity>(
                _connectionManager, 
                tableName, 
                _mappingCollection.GetEntityMapping<TEntity>(mapping.Select(x => new ColumnMapping(x.ColumnName, x.Name))), 
                _profiler, 
                noLock);
        }
    }
}