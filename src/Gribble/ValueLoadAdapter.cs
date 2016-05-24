using System.Collections.Generic;
using System.Linq;

namespace Gribble
{
    public class ValueLoadAdapter<T> : ILoadAdapter<T>
    {
        public T Entity { get; private set; }

        public void SetValues(IDictionary<string, object> values)
        {
            Entity = (T)values.First().Value;
        }

        public IDictionary<string, object> GetValues()
        {
            throw new System.NotImplementedException();
        }
    }
}