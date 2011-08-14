using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Gribble;

namespace Tests
{
    public class TestDatabase
    {
        private readonly string _databaseName = "DB_" + Guid.NewGuid().ToString("N");
        private readonly IList<TestTable> _tables = new List<TestTable>();
        private SqlConnection _connection;

        public TestDatabase() {}

        public TestDatabase(string tableColumnSchema, int recordCount, string tableDataColumns, string tableData)
        {
            AddTable(tableColumnSchema, recordCount, tableDataColumns, tableData);
        }

        public void SetUp() { CreateDatabase(); }

        public void TearDown()
        {
            if (_connection != null) _connection.Dispose();
            DropDatabase();
        }

        public string DatabaseName { get { return _databaseName; } }
        public IList<TestTable> Tables { get { return _tables; } }
        public TestTable FirstTable { get { return _tables[0]; } }
        public TestTable SecondTable { get { return _tables[1]; } }
        public TestTable ThirdTable { get { return _tables[2]; } }
        public TestTable FourthTable { get { return _tables[3]; } }
        public TestTable FifthTable { get { return _tables[4]; } }

        public SqlConnection Connection {get
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(string.Format("server=localhost;database={0};Integrated Security=SSPI", DatabaseName));
                _connection.Open();
            }
            return _connection;
        }}

        public void AddTable(string tableColumnSchema, int recordCount, string tableDataColumns, string tableData)
        {
            _tables.Add(new TestTable(this, tableColumnSchema, recordCount, tableDataColumns, tableData));
        }

        public void AddTable(string tableColumnSchema)
        {
            _tables.Add(new TestTable(this, tableColumnSchema));
        }

        protected virtual void CreateDatabase()
        {
            using (var connection = new SqlConnection("server=localhost;Integrated Security=SSPI"))
            {
                connection.Open();
                new SqlCommand(string.Format("CREATE DATABASE [{0}]", _databaseName), connection).ExecuteNonQuery();
            }
        }

        protected virtual void DropDatabase()
        {
            SqlConnection.ClearAllPools();
            using (var connection = new SqlConnection("server=localhost;Integrated Security=SSPI"))
            {
                connection.Open();
                new SqlCommand("USE master", connection).ExecuteNonQuery();
                new SqlCommand(string.Format("DROP DATABASE [{0}]", _databaseName), connection).ExecuteNonQuery();
            }
        }

        public void ExecuteNonQuery(string format, params object[] args)
        {
            new SqlCommand(string.Format(format, args), Connection).ExecuteNonQuery();
        }

        public T ExecuteScalar<T>(string format, params object[] args)
        {
            return (T)(new SqlCommand(string.Format(format, args), Connection).ExecuteScalar());
        }

        public void CreateTables()
        {
            _tables.ToList().ForEach(x => x.CreateTable());
        }
    }
}
