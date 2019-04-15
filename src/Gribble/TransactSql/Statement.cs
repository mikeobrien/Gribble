using System.Collections.Generic;
using Gribble.Extensions;

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
            Parameters = parameters ?? new Dictionary<string, object>();
        }

        public string Text { get; set; }
        public StatementType Type { get; set; }
        public ResultType Result { get; set; }
        public IDictionary<string, object> Parameters { get; set; }

        public Statement MergeParameters(IDictionary<string, object> parameters)
        {
            parameters.AddRange(Parameters);
            return this;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
