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

        public Database(SqlConnection connection, TimeSpan commandTimeout) :
            this(new ConnectionManager(connection, commandTimeout), new EntityMappingCollection(Enumerable.Empty<IClassMap>()), null) { }

        public Database(SqlConnection connection, TimeSpan commandTimeout, EntityMappingCollection mappingCollection) :
            this(new ConnectionManager(connection, commandTimeout), mappingCollection, null) { }

        public Database(SqlConnection connection, TimeSpan commandTimeout, EntityMappingCollection mappingCollection, IProfiler profiler) :
            this(new ConnectionManager(connection, commandTimeout), mappingCollection, profiler) { }

        public Database(IConnectionManager connectionManager, EntityMappingCollection mappingCollection) :
            this(connectionManager, mappingCollection, null) { }

        public Database(IConnectionManager connectionManager, EntityMappingCollection mappingCollection, IProfiler profiler)
        {
            _connectionManager = connectionManager;
            _profiler = profiler;
            _mappingCollection = mappingCollection;
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

        public void CreateTable(string tableName, params Column[] columns)
        { Command.Create(SchemaWriter.CreateTableCreateStatement(tableName, columns), _profiler).ExecuteNonQuery(_connectionManager); }

        public void DeleteTable(string tableName)
        { Command.Create(SchemaWriter.CreateDeleteTableStatement(tableName), _profiler).ExecuteNonQuery(_connectionManager); }

        public IEnumerable<Column> GetColumns(string tableName)
        {
            throw new NotImplementedException();
        }

        public void AddColumn(string tableName, Column column)
        { Command.Create(SchemaWriter.CreateAddColumnStatement(tableName, column), _profiler).ExecuteNonQuery(_connectionManager); }

        public void RemoveColumn(string tableName, string columnName)
        { Command.Create(SchemaWriter.CreateRemoveColumnStatement(tableName, columnName), _profiler).ExecuteNonQuery(_connectionManager); }

        public IEnumerable<Index> GetIndexes(string tableName)
        {
            throw new NotImplementedException();
        }

        public void AddNonClusteredIndex(string tableName, params string[] columnNames)
        { Command.Create(SchemaWriter.CreateAddNonClusteredIndexStatement(tableName, columnNames), _profiler).ExecuteNonQuery(_connectionManager); }

        public void RemoveNonClusteredIndex(string tableName, string indexName)
        { Command.Create(SchemaWriter.CreateRemoveNonClusteredIndexStatement(tableName, indexName), _profiler).ExecuteNonQuery(_connectionManager); }
    }
}
