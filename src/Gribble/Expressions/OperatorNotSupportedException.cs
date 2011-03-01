using System;
using System.Linq.Expressions;

namespace Gribble.Expressions
{
    public class OperatorNotSupportedException : Exception
    {
        public OperatorNotSupportedException(ExpressionType type) :
            base(string.Format("Operator {0} not supported.", type)) { }
    }
}
