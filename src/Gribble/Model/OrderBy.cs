namespace Gribble.Model
{
    public enum Order
    {
        Ascending,
        Descending
    }

    public class OrderBy
    {
        public enum SourceType { Projection, Operator }

        public SourceType Type;
        public Operator Operator;
        public Projection Projection;
        public Order Order;
    }
}