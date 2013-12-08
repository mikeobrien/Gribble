using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using Gribble.TransactSql;
using System.Data.Common;
using Gribble.Mapping;

namespace Gribble
{
    public class GribbleSPList
    {
        BaseGribbleDB _db;
        public ConnectionManager ConnectionManager { get { return _db.ConnectionManager; } }
        public EntityMappingCollection EntityMapping { get { return _db.EntityMapping; } }

        public GribbleSPList(BaseGribbleDB db)
        {
            _db = db;
        }
    }

    public abstract class BaseGribbleDB : IDisposable
    {
        protected ConnectionManager _connectionManager;
        public ConnectionManager ConnectionManager {get{ return _connectionManager;}}
        protected IProfiler _profiler;

        protected GribbleSPList _sp;
        public GribbleSPList SP { get { return _sp; } }

        public EntityMappingCollection EntityMapping {get {return GetEntityMapping();}}

        public BaseGribbleDB(ConnectionManager connectionManager, IProfiler profiler = null)
        {
            _connectionManager = connectionManager;
            _profiler = profiler ?? new NullProfiler();
            _sp = new GribbleSPList(this);
        }

        public BaseGribbleDB(string connectionString, string providerName, IProfiler profiler = null)
        {
            _connectionManager = new ConnectionManager(connectionString, providerName);
            _profiler = profiler ?? new NullProfiler();
            _sp = new GribbleSPList(this);
        }

        public BaseGribbleDB(string connectionString, DbProviderFactory provider, IProfiler profiler = null)
        {
            _connectionManager = new ConnectionManager(connectionString, provider);
            _profiler = profiler ?? new NullProfiler();
            _sp = new GribbleSPList(this);
        }

        public BaseGribbleDB(IDbConnection connection, IProfiler profiler = null)
        {
            _connectionManager = new ConnectionManager(connection);
            _profiler = profiler ?? new NullProfiler();
            _sp = new GribbleSPList(this);
        }
        
        public BaseGribbleDB(string connectionStringName, IProfiler profiler = null)
        {
            if (ConfigurationManager.ConnectionStrings[connectionStringName] == null)
            {
                throw new Exception("Connection Name not found in Config");
            }

            var providerName = "";
            if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName))
                providerName = ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName;

            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            _connectionManager = new ConnectionManager(connectionString, providerName);
            _profiler = profiler ?? new NullProfiler();
            _sp = new GribbleSPList(this);
        }

        protected abstract EntityMappingCollection GetEntityMapping();

        public int Execute(string sql, object parameters = null)
        {
            return Command.Create(GenericSqlWriter.CreateStatement(sql, parameters.ToDictionary(), Statement.ResultType.None), _profiler).ExecuteNonQuery(_connectionManager);
        }

        public TReturn Execute<TReturn>(string sql, object parameters = null)
        {
            return Command.Create(GenericSqlWriter.CreateStatement(sql, parameters.ToDictionary(), Statement.ResultType.None), _profiler).ExecuteNonQuery<TReturn>(_connectionManager);
        }

        public T ExecuteScalar<T>(string sql, object parameters = null)
        {
            return Command.Create(GenericSqlWriter.CreateStatement(sql, parameters.ToDictionary(), Statement.ResultType.Scalar), _profiler).ExecuteScalar<T>(_connectionManager);
        }

        public TEntity ExecuteSingle<TEntity>(string sql, object parameters = null)
        {
            return Load<TEntity, TEntity>(Command.Create(GenericSqlWriter.CreateStatement(sql, parameters.ToDictionary(), Statement.ResultType.Single), _profiler));
        }

        public TEntity ExecuteSingleOrNone<TEntity>(string sql, object parameters = null)
        {
            return Load<TEntity, TEntity>(Command.Create(GenericSqlWriter.CreateStatement(sql, parameters.ToDictionary(), Statement.ResultType.SingleOrNone), _profiler));
        }

        public IEnumerable<TEntity> ExecuteMany<TEntity>(string sql, object parameters = null)
        {
            return Load<TEntity, IEnumerable<TEntity>>(Command.Create(GenericSqlWriter.CreateStatement(sql, parameters.ToDictionary(), Statement.ResultType.Multiple), _profiler));
        }

        private TResult Load<TEntity, TResult>(Command command)
        {
            return (TResult)new Loader<TEntity>(command, GetEntityMapping().GetEntityMapping<TEntity>()).Execute(_connectionManager);
        }

        public IDbTransaction BeginTransaction()
        {
            return _connectionManager.Connection.BeginTransaction();
        }

        public void Dispose()
        {
            _connectionManager.Dispose();
        }

    }
}
