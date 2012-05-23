namespace Gribble.Model
{
    public class Distinct
    {
        public Projection Projection;
        public OrderBy Order;
        public bool HasOrder { get { return Order != null; } }
    }
}