using System.Collections.Generic;
using System.Reflection;

namespace Gribble.Mapping
{
    public class DynamicMap
    {
        public DynamicMap(PropertyInfo property)
        {
            Property = property;
        }

        public PropertyInfo Property { get; }
        public List<DynamicMapping> Mapping { get; set; } = new List<DynamicMapping>();
    }

    public class DynamicMapping
    {
        public DynamicMapping(string columnName, string name)
        {
            ColumnName = columnName;
            Name = name;
        }

        public string ColumnName { get; }
        public string Name { get; }
        public bool Readonly { get; set; }
    }
}