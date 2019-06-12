using System.Collections.Generic;
using System.Linq;

namespace Gribble.Model
{
    public class Update
    {
        public Update(IEnumerable<KeyValuePair<string, object>> assignement, string table, 
            Operator where, int? top = null, TopValueType? topValueType = null)
        {
            Assignment = assignement.ToDictionary(x => x.Key, x => x.Value);
            Table.Name = table;
            Where = where;
            if (top.HasValue)
            {
                Top = top.Value;
                TopType = topValueType ?? TopValueType.Count;
            }
        }
        
        public int Top;
        public TopValueType TopType;
        public bool HasTop => Top > 0;

        public Table Table = new Table();
        public IDictionary<string, object> Assignment;

        public Operator Where;
        public bool HasWhere => Where != null;
    }
}
