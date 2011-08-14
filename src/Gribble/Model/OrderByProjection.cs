namespace Gribble.Model
{
    public class OrderByProjection
    {
        public enum Ordering
        {
            Ascending,
            Descending
        }

        public Projection Projection;
        public Ordering Order;
    }
}
