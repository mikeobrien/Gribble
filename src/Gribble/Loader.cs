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

        private static IEnumerable<TEntity> LoadMultipleResults(IConnectionManager connectionManager, Command command, IEntityMapping map)
        {
            using (var reader = command.ExecuteReader(connectionManager)) 
                while (reader.Read()) 
                    yield return LoadEntity(reader, map);
        }

        private static TEntity LoadSingleResult(IConnectionManager connectionManager, Command command, IEntityMapping map, bool failOnNoResults)
        {
            using (var reader = command.ExecuteReader(connectionManager))
            {
                if (reader.Read()) return LoadEntity(reader, map);
                if (failOnNoResults) throw new Exception("No result returned for query.");
                return default(TEntity);
            }
        }

        private static TEntity LoadEntity(IDataRecord reader, IEntityMapping map)
        {
            var adapter = new EntityAdapter<TEntity>(map);
            var values = Enumerable.Range(0, reader.FieldCount).
                                    Select(x => new { ColumnName = reader.GetName(x), Value = reader[x].FromDb<object>() }).
                                    ToDictionary(value => value.ColumnName, value => value.Value);
            adapter.SetValues(values);
            return adapter.Entity;
        }
    }
}
