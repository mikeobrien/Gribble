using System.Collections.Generic;
using Gribble.Model;

namespace Gribble
{
    public interface IDatabase
    {
        void CreateTable(string tableName, params Column[] columns);
        void CreateTable(string tableName, string modelTable);
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
