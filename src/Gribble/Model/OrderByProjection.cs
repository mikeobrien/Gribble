namespace Gribble.Model
{
    public enum Order
    {
        Ascending,
        Descending
    }

    public class OrderByProjection
    {
        public Projection Projection;
        public Order Order;
    }
}
