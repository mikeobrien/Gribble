using System;
using System.Collections.Generic;
using System.Data;
using Gribble.Collections;

namespace Gribble.Extensions
{
    public static class AdoExtensions
    {
        public static Dictionary<string, object> ToDictionary(this IDataRecord dataRecord)
        {
            return dataRecord.FieldCount.ToRange().ToDistinctDictionary(dataRecord.GetName, 
                x => dataRecord[x].FromDb<object>(), StringComparer.OrdinalIgnoreCase);
        }

        public static IDictionary<string, object> ToDefaultValueDictionary(this IDataRecord dataRecord)
        {
            return new DefaultValueDictionary<string, object>(dataRecord.ToDictionary());
        }
    }
}
