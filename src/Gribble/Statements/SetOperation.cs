namespace Gribble.Statements
{
    public class SetOperation
    {
        public enum OperationType
        {
            Intersect,
            Compliment
        }

        public OperationType Type;
        public Select Select;
    }
}
