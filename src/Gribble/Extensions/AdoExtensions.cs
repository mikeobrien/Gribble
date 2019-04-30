using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Gribble.Extensions
{
    public static class AdoExtensions
    {
        public static IDictionary<string, object> ToDictionary(this IDataRecord dataRecord)
        {
            return Enumerable.Range(0, dataRecord.FieldCount).
                Select(x => new
                {
                    ColumnName = dataRecord.GetName(x),
                    Value = dataRecord[x].FromDb<object>()
                }).
                ToDictionary(value => value.ColumnName, value => value.Value);
        }
    }
}
