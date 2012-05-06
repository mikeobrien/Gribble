using System.Collections.Generic;

namespace Gribble.Model
{
    public class Index
    {
        public Index(string name, bool clustered, bool unique, bool primaryKey, IEnumerable<IndexColumn> columns)
        {
            Name = name;
            Clustered = clustered;
            Unique = unique;
            PrimaryKey = primaryKey;
            Columns = columns;
        }

        public string Name { get; private set; }
        public bool Clustered { get; private set; }
        public bool Unique { get; private set; }
        public bool PrimaryKey { get; private set; }
        public IEnumerable<IndexColumn> Columns { get; private set; }

        public class IndexColumn
        {
            public IndexColumn(string name, bool descending)
            {
                Name = name;
                Descending = descending;
            }

            public string Name { get; private set; }
            public bool Descending { get; private set; }
        }
    }
}