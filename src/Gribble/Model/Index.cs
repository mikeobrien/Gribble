using System.Collections.Generic;

namespace Gribble.Model
{
    public class Index
    {
        public Index(string name, bool clustered, bool unique, bool primaryKey, ColumnSet columns)
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
        public ColumnSet Columns { get; private set; }

        public class ColumnSet : List<Column>
        {
            public ColumnSet Add(string name, bool descending = false)
            {
                Add(new Column(name, descending));
                return this;
            }
        }

        public class Column
        {
            public Column(string name, bool descending = false)
            {
                Name = name;
                Descending = descending;
            }

            public string Name { get; private set; }
            public bool Descending { get; private set; }
        }
    }
}