using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gribble.Collections;

namespace Gribble.Extensions
{
    public static class AdoExtensions
    {
        public static Dictionary<string, object> ToDictionary(this IDataRecord dataRecord)
        {
            return Enumerable
                .Range(0, dataRecord.FieldCount)
                .Select(x => new
                {
                    ColumnName = dataRecord.GetName(x),
                    Value = dataRecord[x].FromDb<object>()
                })
                .ToDictionary(value => value.ColumnName, value => value.Value);
        }

        public static IDictionary<string, object> ToDefaultValueDictionary(this IDataRecord dataRecord)
        {
            return new DefaultValueDictionary<string, object>(dataRecord.ToDictionary());
        }
    }
}
