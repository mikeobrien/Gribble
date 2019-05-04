namespace Gribble.Mapping
{
    public class DynamicMapping
    {
        public DynamicMapping(string columnName, string name)
        {
            ColumnName = columnName;
            Name = name;
        }

        public string ColumnName { get; }
        public string Name { get; }
    }
}
