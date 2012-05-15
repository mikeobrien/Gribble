namespace Gribble.Model
{
    public class Duplicates
    {
        public Projection Field;
        public Operator Precedence;
        public bool HasPrecedence { get { return Precedence != null; } }
    }
}