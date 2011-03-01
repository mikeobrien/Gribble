using System.Collections.Generic;

namespace Gribble.TransactSql
{
    public class Statement
    {
        public enum ResultType
        {
            None,
            Single,
            SingleOrNone,
            Multiple,
            Scalar
        }

        public enum StatementType
        {
            Text,
            StoredProcedure
        }

        public Statement(string text, StatementType type, IDictionary<string, object> parameters) :
            this(text, type, ResultType.None, parameters) { }

        public Statement(string text, StatementType type, ResultType result) :
            this(text, type, result, new Dictionary<string, object>()) { }

        public Statement(string text, StatementType type, ResultType result, IDictionary<string, object> parameters)
        {
            Type = type;
            Text = text;
            Result = result;
            Parameters = parameters;
        }

        public string Text;
        public StatementType Type;
        public ResultType Result;
        public IDictionary<string, object> Parameters;
    }
}
