namespace Gribble.Model
{
    public class Operand
    {
        public enum OperandType
        {
            Operator,
            Projection
        }

        public OperandType Type;

        public Projection Projection;
        public Operator Operator;

        public static class Create
        {
            public static Operand Projection(Projection projection)
            {
                return new Operand { Type = OperandType.Projection, Projection = projection};
            }

            public static Operand Operator(Operator @operator)
            {
                return new Operand { Type = OperandType.Operator, Operator = @operator };
            }
        }
    }
}
