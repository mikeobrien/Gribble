namespace Gribble.Model
{
    public class Delete
    {
        public enum FilterType { Select, Where }

        public Delete(string table, Operator where, bool allowMultiple)
        {
            Table.Name = table;
            AllowMultiple = allowMultiple;
            Where = where;
            Filter = FilterType.Where;
        }

        public Delete(string table, Select select, bool allowMultiple)
        {
            Table.Name = table;
            AllowMultiple = allowMultiple;
            Select = select;
            Filter = FilterType.Select;
        }

        public Table Table = new Table();
        public bool AllowMultiple;
        public FilterType Filter;
        public Select Select;
        public Operator Where;
    }
}
