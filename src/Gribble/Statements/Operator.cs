namespace Gribble.Statements
{
    public class Operator
    {
        public enum OperatorType
        {
            // Logical
            And,
            Or,
            // Compaison
            Equal,
            NotEqual,
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
            // Math
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulo
        }

        public OperatorType Type;
        public Operand LeftOperand;
        public Operand RightOperand;

        public static class Create
        {
            public static Operator ByType(OperatorType type)
            {
                return new Operator { Type = type };
            }

            public static Operator RightEquals(Operand rightOperand)
            {
                return new Operator { Type = OperatorType.Equal, RightOperand = rightOperand };
            }

            public static Operator Equal(Operand leftOperand, Operand rightOperand)
            {
                return new Operator { Type = OperatorType.Equal, LeftOperand = leftOperand, RightOperand = rightOperand };
            }

            public static Operator FieldEqualsConstant(string fieldName, object value)
            {
                return FieldAndConstant(fieldName, OperatorType.Equal, value);
            }

            public static Operator FieldAndConstant(string fieldName, OperatorType @operator, object value)
            {
                return new Operator
                {
                    LeftOperand = Operand.Create.Projection(Projection.Create.Field(fieldName)),
                    Type = @operator,
                    RightOperand = Operand.Create.Projection(Projection.Create.Constant(value))
                };
            }

            public static Operator Operators(Operator left, OperatorType @operator, Operator right)
            {
                return new Operator
                {
                    LeftOperand = Operand.Create.Operator(left),
                    Type = @operator,
                    RightOperand = Operand.Create.Operator(right)
                };
            }
        }
    }
}
