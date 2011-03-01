using System;
using System.Collections.Generic;
using System.Linq;

namespace Gribble.Statements
{
    public class Data
    {
        private static readonly Random Random = new Random();

        public enum DataType
        {
            Query,
            Table
        }

        public DataType Type = DataType.Table;
        public IList<Select> Queries;
        public Table Table;
        public string Alias = string.Format("T{0}", Random.Next());

        public bool IsTable { get { return Type == DataType.Table; } }

        public bool HasQueries
        { get { return Type == DataType.Query && Queries != null && Queries.Any(); } }
    }
}
