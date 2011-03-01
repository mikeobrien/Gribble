using System.Collections.Generic;
using System.Linq;

namespace Gribble.Statements
{
    public class Update
    {
        public Update(IEnumerable<KeyValuePair<string, object>> assignement, string table, Operator where)
        {
            Assignment = assignement.ToDictionary(x => x.Key, x => x.Value);
            Table.Name = table;
            Where = where;
        }

        public Table Table = new Table();
        public IDictionary<string, object> Assignment;

        public Operator Where;
        public bool HasWhere { get { return Where != null; } }
    }
}
