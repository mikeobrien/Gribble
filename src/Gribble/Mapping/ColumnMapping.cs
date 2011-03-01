namespace Gribble.Mapping
{
    public class ColumnMapping
    {
        public ColumnMapping(string columnName, string name)
        {
            ColumnName = columnName;
            Name = name;
        }

        public string ColumnName { get; private set; }
        public string Name { get; private set; }
    }
}
