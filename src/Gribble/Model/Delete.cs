namespace Gribble.Model
{
    public class Delete
    {
        public Delete(string table, Operator where, bool allowMultiple)
        {
            Table.Name = table;
            AllowMultiple = allowMultiple;
            Where = where;
        }

        public Table Table = new Table();
        public bool AllowMultiple;
        public Operator Where;
        public bool HasWhere { get { return Where != null; } }
    }
}
