using System.Reflection;

namespace Gribble.Mapping
{
    public class PropertyMapping
    {
        public PropertyMapping(string columnName, PropertyInfo property)
        {
            ColumnName = columnName;
            Property = property;
        }

        public string ColumnName { get; }
        public PropertyInfo Property { get; }
        public bool Readonly { get; set; }
    }

    public class KeyPropertyMapping : PropertyMapping
    {
        public KeyPropertyMapping(string columnName, PropertyInfo property, 
            PrimaryKeyType type) : base(columnName, property)
        {
            Type = type;
        }

        public PrimaryKeyType Type { get; }
        public PrimaryKeyGeneration Generation { get; set; }
    }
}