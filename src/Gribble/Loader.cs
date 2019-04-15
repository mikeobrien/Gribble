using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.TransactSql;

namespace Gribble
{
    public class Loader<TEntity>
    {
        private readonly Command _command;
        private readonly IEntityMapping _map;

        public Loader(Command command, IEntityMapping map)
        {
            _command = command;
            _map = map;
        }

        public object Execute(IConnectionManager connectionManager)
        {
            switch (_command.Statement.Result)
            {
                case Statement.ResultType.Multiple: return LoadMultipleResults(connectionManager, _command, _map);
                case Statement.ResultType.Single: return LoadSingleResult(connectionManager, _command, _map, true);
                case Statement.ResultType.SingleOrNone: return LoadSingleResult(connectionManager, _command, _map, false);
                case Statement.ResultType.Scalar: return _command.ExecuteScalar(connectionManager);
                default: { _command.ExecuteNonQuery(connectionManager); return null; }
            }
        }

        private static IEnumerable<TEntity> LoadMultipleResults(IConnectionManager connectionManager, 
            Command command, IEntityMapping map)
        {
            using (var reader = command.ExecuteReader(connectionManager)) 
                while (reader.Read()) 
                    yield return LoadEntity(reader, map);
        }

        private static TEntity LoadSingleResult(IConnectionManager connectionManager, 
            Command command, IEntityMapping map, bool failWhenNotSingleResult)
        {
            var results = LoadMultipleResults(connectionManager, command, map).ToList();
            if (failWhenNotSingleResult && !results.Any())
                throw new Exception("No result returned for query.");
            if (failWhenNotSingleResult && results.Count > 1)
                throw new Exception("More than one result returned for query.");
            return results.FirstOrDefault();
        }

        private static TEntity LoadEntity(IDataRecord record, IEntityMapping map)
        {
            var adapter = typeof(TEntity).IsSimpleType() ? 
                (ILoadAdapter<TEntity>)new ValueLoadAdapter<TEntity>() :
                new EntityAdapter<TEntity>(map);
            var values = Enumerable.Range(0, record.FieldCount).
                Select(x => new { ColumnName = record.GetName(x), Value = record[x].FromDb<object>() }).
                ToDictionary(value => value.ColumnName, value => value.Value);
            adapter.SetValues(values);
            return adapter.Entity;
        }
    }
}
