using System.Collections.Generic;
using System.Linq;

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

        public bool IsEquivalent(Index index)
        {
            var columns = Columns.Join(index.Columns, x => x.Name, x => x.Name, (a, b) => new {a, b}).ToList();
            return Clustered == index.Clustered &&
                Unique == index.Unique &&
                PrimaryKey == index.PrimaryKey &&
                columns.Count == Columns.Count &&
                columns.All(x => x.a.Descending == x.b.Descending);
        }

        public class ColumnSet : List<Column>
        {
            public ColumnSet() {}

            public ColumnSet(IEnumerable<Column> columns)
            {
                AddRange(columns);
            }

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