using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Gribble.Mapping;
using Gribble.Model;
using Gribble.TransactSql;

namespace Gribble
{
    public class Database : IDatabase
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IProfiler _profiler;
        private readonly EntityMappingCollection _mappingCollection;

        public Database(IConnectionManager connectionManager) :
            this(connectionManager, new EntityMappingCollection(Enumerable.Empty<IClassMap>()), null) { }

        public Database(IConnectionManager connectionManager, EntityMappingCollection mappingCollection) :
            this(connectionManager, mappingCollection, null) { }

        private Database(IConnectionManager connectionManager, EntityMappingCollection mappingCollection, IProfiler profiler)
        {
            _connectionManager = connectionManager;
            _profiler = profiler;
            _mappingCollection = mappingCollection;
        }

        public static IDatabase Create(SqlConnection connection, TimeSpan commandTimeout)
        {
            return new Database(new ConnectionManager(connection, commandTimeout), new EntityMappingCollection(Enumerable.Empty<IClassMap>()), null);
        }

        public static IDatabase Create(SqlConnection connection, TimeSpan commandTimeout, EntityMappingCollection mappingCollection)
        {
            return new Database(new ConnectionManager(connection, commandTimeout), mappingCollection, null);
        }

        public static IDatabase Create(SqlConnection connection, TimeSpan commandTimeout, EntityMappingCollection mappingCollection, IProfiler profiler)
        {
            return new Database(new ConnectionManager(connection, commandTimeout), mappingCollection, profiler);
        }
        
        public static IDatabase Create(IConnectionManager connectionManager, EntityMappingCollection mappingCollection, IProfiler profiler)
        {
            return new Database(connectionManager, mappingCollection, profiler);
        }

        public void CallProcedure(string name)
        { Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.None), _profiler).ExecuteNonQuery(_connectionManager); }

        public void CallProcedure(string name, Dictionary<string, object> parameters)
        { Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.None), _profiler).ExecuteNonQuery(_connectionManager); }

        public T CallProcedureScalar<T>(string name)
        { return Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.Scalar), _profiler).ExecuteScalar<T>(_connectionManager); }

        public T CallProcedureScalar<T>(string name, Dictionary<string, object> parameters)
        { return Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.Scalar), _profiler).ExecuteScalar<T>(_connectionManager); }

        public TEntity CallProcedureSingle<TEntity>(string name)
        { return Load<TEntity, TEntity>(Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.Single), _profiler)); }

        public TEntity CallProcedureSingle<TEntity>(string name, Dictionary<string, object> parameters)
        { return Load<TEntity, TEntity>(Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.Single), _profiler)); }

        public TEntity CallProcedureSingleOrNone<TEntity>(string name)
        { return Load<TEntity, TEntity>(Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.SingleOrNone), _profiler)); }

        public TEntity CallProcedureSingleOrNone<TEntity>(string name, Dictionary<string, object> parameters)
        { return Load<TEntity, TEntity>(Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.SingleOrNone), _profiler)); }

        public IEnumerable<TEntity> CallProcedureMany<TEntity>(string name)
        { return Load<TEntity, IEnumerable<TEntity>>(Command.Create(StoredProcedureWriter.CreateStatement(name, Statement.ResultType.Multiple), _profiler)); }

        public IEnumerable<TEntity> CallProcedureMany<TEntity>(string name, Dictionary<string, object> parameters)
        { return Load<TEntity, IEnumerable<TEntity>>(Command.Create(StoredProcedureWriter.CreateStatement(name, parameters, Statement.ResultType.Multiple), _profiler)); }

        private TResult Load<TEntity, TResult>(Command command)
        { return (TResult)new Loader<TEntity>(command, _mappingCollection.GetEntityMapping<TEntity>()).Execute(_connectionManager); }

        public bool TableExists(string tableName)
        { return Command.Create(SchemaWriter.CreateTableExistsStatement(tableName), _profiler).ExecuteScalar<bool>(_connectionManager); }

        public void CreateTable(string tableName, params Column[] columns)
        { Command.Create(SchemaWriter.CreateTableCreateStatement(tableName, columns), _profiler).ExecuteNonQuery(_connectionManager); }

        public void DeleteTable(string tableName)
        { Command.Create(SchemaWriter.CreateDeleteTableStatement(tableName), _profiler).ExecuteNonQuery(_connectionManager); }

        
        public IEnumerable<Column> GetColumns(string tableName)
        {
            return GetColumns(SchemaWriter.CreateTableColumnsStatement(tableName));
        }

        internal IEnumerable<Column> GetColumns(Statement statement)
        {
            var columns = new List<Column>();
            using(var reader = Command.Create(statement, _profiler).ExecuteReader(_connectionManager))
            {
                while (reader.Read())
                {
                    columns.Add(new Column(
                        (string)reader["name"],
                        type: ((byte)reader["system_type_id"]).GetClrType((bool)reader["is_nullable"]),
                        sqlType: ((byte)reader["system_type_id"]).GetSqlType(),
                        length: (short)reader["max_length"],
                        isNullable: (bool)reader["is_nullable"],
                        isIdentity: (bool)reader["is_identity"],
                        isAutoGenerated: (bool)reader["is_auto_generated"],
                        key: !(bool)reader["is_primary_key"] ? 
                            Column.KeyType.None : 
                            ((bool)reader["is_primary_key_clustered"] ? 
                                Column.KeyType.ClusteredPrimaryKey : 
                                Column.KeyType.PrimaryKey),
                        defaultValue: reader["default_value"].FromDb<object>(),
                        precision: (byte)reader["precision"],
                        scale: (byte)reader["scale"],
                        computationPersisted: reader["persisted_computation"].FromDb<bool?>(),
                        computation: reader["computation"].FromDb<string>()));
                }
            }
            return columns;
        }

        public void AddColumn(string tableName, Column column)
        { Command.Create(SchemaWriter.CreateAddColumnStatement(tableName, column), _profiler).ExecuteNonQuery(_connectionManager); }

        public void AddColumns(string tableName, params Column[] columns)
        {
            var existingColumns = GetColumns(tableName);
            columns.Where(x => !existingColumns.Any(y => y.Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase))).
                ToList().ForEach(x => AddColumn(tableName, x));
        }

        public void RemoveColumn(string tableName, string columnName)
        { Command.Create(SchemaWriter.CreateRemoveColumnStatement(tableName, columnName), _profiler).ExecuteNonQuery(_connectionManager); }

        public IEnumerable<Index> GetIndexes(string tableName)
        {
            var indexes = new List<Index>();
            using (var reader = Command.Create(SchemaWriter.CreateGetIndexesStatement(tableName), _profiler).ExecuteReader(_connectionManager))
            {
                Index index = null;
                IList<Index.IndexColumn> columns = null;
                while (reader.Read())
                {
                    var name = (string)reader["name"];
                    if (index == null || index.Name != name)
                    {
                        columns = new List<Index.IndexColumn>();
                        index = new Index(name,
                            (byte)reader["type"] == 1,
                            (bool)reader["is_unique"],
                            (bool)reader["is_primary_key"], columns);
                        indexes.Add(index);
                    }
                    columns.Add(new Index.IndexColumn(
                        (string)reader["column_name"],
                        (bool)reader["is_descending_key"]));
                }
            }
            return indexes;
        }

        public void AddNonClusteredIndex(string tableName, params string[] columnNames)
        { Command.Create(SchemaWriter.CreateAddNonClusteredIndexStatement(tableName, columnNames), _profiler).ExecuteNonQuery(_connectionManager); }

        public void AddNonClusteredIndexes(string tableName, params string[][] indexes)
        {
            var existingIndexes = GetIndexes(tableName).Select(x => x.Columns.Select(y => y.Name).OrderBy(y => y));
            indexes.Where(x => !existingIndexes.Any(y => y.SequenceEqual(x.OrderBy(z => z), StringComparer.OrdinalIgnoreCase))).
                ToList().ForEach(x => AddNonClusteredIndex(tableName, x));
        }

        public void RemoveNonClusteredIndex(string tableName, string indexName)
        { Command.Create(SchemaWriter.CreateRemoveNonClusteredIndexStatement(tableName, indexName), _profiler).ExecuteNonQuery(_connectionManager); }

    }
}
