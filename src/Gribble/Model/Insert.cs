using System.Collections.Generic;

namespace Gribble.Model
{
    public class Insert
    {
        public enum SetType
        {
            Values,
            Query
        }

        public Table Into = new Table();
        public SetType Type = SetType.Values;
        public IDictionary<string, object> Values;
        public bool HasIdentityKey;
        public Select Query;
    }
}
