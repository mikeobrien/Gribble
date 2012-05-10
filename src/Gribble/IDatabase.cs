using System.Collections.Generic;
using Gribble.Model;

namespace Gribble
{
    public interface IDatabase
    {
        void CallProcedure(string name);
        void CallProcedure(string name, Dictionary<string, object> parameters);
        T CallProcedureScalar<T>(string name);
        T CallProcedureScalar<T>(string name, Dictionary<string, object> parameters);
        TEntity CallProcedureSingle<TEntity>(string name);
        TEntity CallProcedureSingle<TEntity>(string name, Dictionary<string, object> parameters);
        TEntity CallProcedureSingleOrNone<TEntity>(string name);
        TEntity CallProcedureSingleOrNone<TEntity>(string name, Dictionary<string, object> parameters);
        IEnumerable<TEntity> CallProcedureMany<TEntity>(string name);
        IEnumerable<TEntity> CallProcedureMany<TEntity>(string name, Dictionary<string, object> parameters);

        void CreateTable(string tableName, params Column[] columns);
        bool TableExists(string tableName);
        void DeleteTable(string tableName);

        IEnumerable<Column> GetColumns(string tableName);
        void AddColumn(string tableName, Column column);
        void AddColumns(string tableName, params Column[] columns);
        void RemoveColumn(string tableName, string columnName);

        IEnumerable<Index> GetIndexes(string tableName);
        void AddNonClusteredIndex(string tableName, params Index.Column[] columns);
        void AddNonClusteredIndexes(string tableName, params Index.ColumnSet[] indexColumns);
        void RemoveNonClusteredIndex(string tableName, string indexName);
    }
}
