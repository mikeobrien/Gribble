using System;
using System.Collections.Generic;
using System.Linq;
using Gribble.Extensions;

namespace Gribble.Model
{
    public class Data
    {
        public enum DataType
        {
            Query,
            Table
        }

        public DataType Type = DataType.Table;
        public IList<Select> Queries;
        public Table Table;
        public string Alias = string.Format("T{0}", Unique.Next());

        public bool IsTable { get { return Type == DataType.Table; } }

        public bool HasQueries
        { get { return Type == DataType.Query && Queries != null && Queries.Any(); } }
    }
}
