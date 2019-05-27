using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Gribble.Extensions;
using Gribble.Model;
using Gribble.TransactSql;

namespace Gribble
{
    public interface ITableSchema
    {
        TableInfo GetTableInfo(string tableName);
        void CreateTable(string tableName, params Column[] columns);
        void CreateTable(string tableName, string modelTable);
        bool TableExists(string tableName);
        void RenameTable(string oldName, string newName);
        void DeleteTable(string tableName);

        List<Column> GetColumns(string tableName);
        Column GetColumn(string tableName, string columnName);
        void AddColumn(string tableName, Column column);
        void AddColumns(string tableName, params Column[] columns);
        void RemoveColumn(string tableName, string columnName);

        List<Index> GetIndexes(string tableName);
        void AddNonClusteredIndex(string tableName, string indexName, params Index.Column[] columns);
        void AddNonClusteredIndex(string tableName, string indexName, params Column[] columns);
        void AddNonClusteredIndex(string tableName, params Column[] columns);
        void AddNonClusteredIndex(string tableName, params Index.Column[] columns);
        void AddNonClusteredIndexes(string tableName, params Index.ColumnSet[] indexColumns);
        void RemoveNonClusteredIndex(string tableName, string indexName);
    }

    public class TableInfo
    {
        public TableInfo(string name, int objectId, bool usesAnsiNulls, DateTime created, DateTime? modified)
        {
            Name = name;
            ObjectId = objectId;
            UsesAnsiNulls = usesAnsiNulls;
            Created = created;
            Modified = modified;
        }

        public string Name { get; }
        public int ObjectId { get; }
        public bool UsesAnsiNulls { get; }
        public DateTime Created { get; }
        public DateTime? Modified { get; }
    }

    public class TableSchema : ITableSchema
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IProfiler _profiler;

        public TableSchema(IConnectionManager connectionManager, IProfiler profiler)
        {
            _connectionManager = connectionManager;
            _profiler = profiler;
        }

        public static ITableSchema Create(SqlConnection connection, TimeSpan? commandTimeout = null, IProfiler profiler = null)
        {
            return Create(new ConnectionManager(connection, commandTimeout ?? new TimeSpan(0, 5, 0)), profiler);
        }

        public static ITableSchema Create(IConnectionManager connectionManager, IProfiler profiler = null)
        {
            return new TableSchema(connectionManager, profiler ?? new ConsoleProfiler());
        }

        public bool TableExists(string tableName)
        {
            return Command.Create(SchemaWriter.CreateTableExistsStatement(tableName), _profiler)
                .ExecuteScalar<bool>(_connectionManager);
        }

        public void RenameTable(string oldName, string newName)
        {
            Command.Create(new Statement("sp_rename", Statement.StatementType.StoredProcedure, 
                new Dictionary<string, object>
                {
                    { "objname", oldName },
                    { "newname", newName }
                }), _profiler)
                .ExecuteNonQuery(_connectionManager);
        }

        public TableInfo GetTableInfo(string tableName)
        {
            var statement = SchemaWriter.CreateTableInfoStatement(tableName);
            using (var reader = Command.Create(statement, _profiler).ExecuteReader(_connectionManager))
            {
                if (reader.Read())
                {
                    return new TableInfo(
                        (string)reader[TransactSql.System.Tables.Name],
                        (int)reader[TransactSql.System.Tables.ObjectId],
                        (bool)reader[TransactSql.System.Tables.UsesAnsiNulls],
                        (DateTime)reader[TransactSql.System.Tables.CreateDate],
                        (DateTime?)reader[TransactSql.System.Tables.ModifyDate]);
                }
            }

            return null;
        }

        public void CreateTable(string tableName, params Column[] columns)
        {
            Command.Create(SchemaWriter.CreateTableCreateStatement(tableName, columns), _profiler)
                .ExecuteNonQuery(_connectionManager);
        }

        public void CreateTable(string tableName, string modelTable)
        {
            var columns = GetColumns(modelTable).ToList();
            var indexes = GetIndexes(modelTable).Where(x => !x.Clustered).ToList();
            CreateTable(tableName, columns.ToArray());
            AddNonClusteredIndexes(tableName, indexes.Select(x => new Index.ColumnSet(x.Columns)).ToArray());
        }

        public void DeleteTable(string tableName)
        {
            Command.Create(SchemaWriter.CreateDeleteTableStatement(tableName), _profiler)
                .ExecuteNonQuery(_connectionManager);
        }

        public List<Column> GetColumns(string tableName)
        {
            var statement = SchemaWriter.CreateTableColumnsStatement(tableName);
            var columns = new List<Column>();
            using (var reader = Command.Create(statement, _profiler).ExecuteReader(_connectionManager))
                while (reader.Read()) columns.Add(ColumnFactory(reader)); 
            return columns;
        }

        public Column GetColumn(string tableName, string columnName)
        {
            var statement = SchemaWriter.CreateTableColumnStatement(tableName, columnName);
            var columns = new List<Column>();
            using (var reader = Command.Create(statement, _profiler).ExecuteReader(_connectionManager))
                while (reader.Read()) columns.Add(ColumnFactory(reader)); 
            return columns.FirstOrDefault();
        }

        internal static Column ColumnFactory(IDataReader reader)
        {
            return new Column(
                (string)reader[TransactSql.System.Columns.Name],
                type: ((byte)reader[TransactSql.System.Columns.SystemTypeId]).GetClrType((bool)reader[TransactSql.System.Columns.IsNullable]),
                sqlType: ((byte)reader[TransactSql.System.Columns.SystemTypeId]).GetSqlType(),
                length: (short)reader[TransactSql.System.Columns.MaxLength],
                isNullable: (bool)reader[TransactSql.System.Columns.IsNullable],
                isIdentity: (bool)reader[TransactSql.System.Columns.IsIdentity],
                isAutoGenerated: (bool)reader[SqlWriter.Aliases.IsAutoGenerated],
                key: !(bool)reader[TransactSql.System.Indexes.IsPrimaryKey] ? 
                    Column.KeyType.None : 
                    ((bool)reader[SqlWriter.Aliases.IsPrimaryKeyClustered] ? 
                        Column.KeyType.ClusteredPrimaryKey : 
                        Column.KeyType.PrimaryKey),
                defaultValue: reader[SqlWriter.Aliases.DefaultValue].FromDb<object>(),
                precision: (byte)reader[TransactSql.System.Columns.Precision],
                scale: (byte)reader[TransactSql.System.Columns.Scale],
                computationPersisted: reader[SqlWriter.Aliases.PersistedComputation].FromDb<bool?>(),
                computation: reader[SqlWriter.Aliases.Computation].FromDb<string>());
        }

        public void AddColumn(string tableName, Column column)
        {
            Command.Create(SchemaWriter.CreateAddColumnStatement(tableName, column), _profiler)
                .ExecuteNonQuery(_connectionManager);
        }

        public void AddColumns(string tableName, params Column[] columns)
        {
            var existingColumns = GetColumns(tableName);
            columns.Where(x => !existingColumns.Any(y => y.Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase))).
                ToList().ForEach(x => AddColumn(tableName, x));
        }

        public void RemoveColumn(string tableName, string columnName)
        { Command.Create(SchemaWriter.CreateRemoveColumnStatement(tableName, columnName), _profiler).ExecuteNonQuery(_connectionManager); }

        public List<Index> GetIndexes(string tableName)
        {
            var indexes = new List<Index>();
            using (var reader = Command.Create(SchemaWriter.CreateGetIndexesStatement(tableName), _profiler).ExecuteReader(_connectionManager))
            {
                Index index = null;
                Index.ColumnSet columns = null;
                while (reader.Read())
                {
                    var name = (string)reader[TransactSql.System.Indexes.Name];
                    if (index == null || index.Name != name)
                    {
                        columns = new Index.ColumnSet();
                        index = new Index(name,
                            (byte)reader[TransactSql.System.Indexes.Type] == 1,
                            (bool)reader[TransactSql.System.Indexes.IsUnique],
                            (bool)reader[TransactSql.System.Indexes.IsPrimaryKey], columns);
                        indexes.Add(index);
                    }
                    columns.Add(
                        (string)reader[SqlWriter.Aliases.ColumnName],
                        (bool)reader[TransactSql.System.IndexColumns.IsDescendingKey]);
                }
            }
            return indexes;
        }

        public void AddNonClusteredIndex(string tableName, string indexName, params Index.Column[] columns)
        { 
            Command.Create(SchemaWriter.CreateAddNonClusteredIndexStatement(tableName, indexName, columns), _profiler).ExecuteNonQuery(_connectionManager);
        }

        public void AddNonClusteredIndex(string tableName, string indexName, params Column[] columns)
        {
            AddNonClusteredIndex(tableName, indexName, columns.Select(x => new Index.Column(x.Name)).ToArray());
        }

        public void AddNonClusteredIndex(string tableName, params Index.Column[] columns)
        { 
            Command.Create(SchemaWriter.CreateAddNonClusteredIndexStatement(tableName, columns), _profiler).ExecuteNonQuery(_connectionManager);
        }

        public void AddNonClusteredIndex(string tableName, params Column[] columns)
        {
            AddNonClusteredIndex(tableName, columns.Select(x => new Index.Column(x.Name)).ToArray());
        }

        public void AddNonClusteredIndexes(string tableName, params Index.ColumnSet[] indexColumns)
        {
            var existingIndexes = GetIndexes(tableName).Select(x => x.Columns.Select(y => y.Name).OrderBy(y => y));
            indexColumns.Where(x => !existingIndexes.Any(y => y.SequenceEqual(x.Select(z => z.Name).OrderBy(z => z), StringComparer.OrdinalIgnoreCase))).
                ToList().ForEach(x => AddNonClusteredIndex(tableName, x.ToArray()));
        }

        public void RemoveNonClusteredIndex(string tableName, string indexName)
        {
            Command.Create(SchemaWriter.CreateRemoveNonClusteredIndexStatement(tableName, indexName), _profiler).ExecuteNonQuery(_connectionManager);
        }
    }
}
