using System.Collections.Generic;

namespace Gribble
{
    public interface ILoadAdapter<T>
    {
        T Entity { get; }
        void SetValues(IDictionary<string, object> values);
    }
}