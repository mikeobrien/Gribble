using System.Collections.Generic;
using System.Linq;

namespace Gribble.Statements
{
    public class Insert
    {
        public enum InsertType
        {
            Record,
            Query
        }

        public Insert(IEnumerable<KeyValuePair<string, object>> assignment, bool hasIdentityKey, string tableName)
        {
            Assignment = assignment.ToDictionary(x => x.Key, x => x.Value);
            HasIdentityKey = hasIdentityKey;
            Table.Name = tableName;
            Type = InsertType.Record;
        }

        public Insert(Select select, IEnumerable<string> fields, string tableName)
        {
            Assignment = fields.ToDictionary<string, string, object>(x => x, x => null);
            Table.Name = tableName;
            Query = select;
            Type = InsertType.Query;
        }

        public Table Table = new Table();
        public InsertType Type = InsertType.Record;
        public IDictionary<string, object> Assignment;
        public bool HasIdentityKey;
        public Select Query;
    }
}
