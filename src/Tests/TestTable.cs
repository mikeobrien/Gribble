using System;
using System.Data.SqlClient;
using System.Linq;

namespace Tests
{
    public class TestTable
    {
        private readonly TestDatabase _database;
        private readonly string _name = GenerateName();
        private readonly string _tableColumnSchema;
        private readonly int _recordCount;
        private readonly string _tableDataColumns;
        private readonly string _tableData;

        public TestTable(TestDatabase database, string tableColumnSchema)
        {
            _database = database;
            _tableColumnSchema = tableColumnSchema;
        }

        public TestTable(TestDatabase database, string tableColumnSchema, int recordCount, string tableDataColumns, string tableData)
        {
            _database = database;
            _recordCount = recordCount;
            _tableColumnSchema = tableColumnSchema;
            _tableDataColumns = tableDataColumns;
            _tableData = tableData;
        }

        public string Name { get { return _name; } }

        public static string GenerateName()
        {
            return "TBL_" + Guid.NewGuid().ToString("N");
        }

        public void CreateTable()
        {
            new SqlCommand(string.Format("IF EXISTS (SELECT * FROM sys.tables WHERE name='{0}') DROP TABLE [{0}]", _name), _database.Connection).ExecuteNonQuery();
            new SqlCommand(string.Format("CREATE TABLE [{0}] ({1})", _name, _tableColumnSchema), _database.Connection).ExecuteNonQuery();
            Enumerable.Range(0, _recordCount).ToList().ForEach(x => new SqlCommand(string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})",
                                                                            _name, _tableDataColumns, _tableData), _database.Connection).ExecuteNonQuery());
        }
    }
}
