namespace Gribble.Model
{
    public class Duplicates
    {
        public enum DuplicateGrouping { DistinctField, Precedence, OrderField }

        public Projection DistinctField;
        public Operator Precedence;
        public Projection OrderField;
        public Order Order;

        public DuplicateGrouping Grouping;
    }
}