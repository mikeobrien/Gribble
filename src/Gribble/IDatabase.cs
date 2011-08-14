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
        void DeleteTable(string tableName);
        void AddColumn(string tableName, Column column);
        void RemoveColumn(string tableName, string columnName);
        void AddNonClusteredIndex(string tableName, string indexName, params string[] columnNames);
        void RemoveNonClusteredIndex(string tableName, string indexName);
    }
}
