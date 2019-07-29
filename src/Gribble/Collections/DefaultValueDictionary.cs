using System.Collections.Generic;

namespace Gribble.Collections
{
    public class DefaultValueDictionary<TKey, TValue> : DictionaryBase<TKey, TValue>
    {
        public DefaultValueDictionary() { }
        public DefaultValueDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        public override TValue this[TKey key]
        {
            get
            {
                base.TryGetValue(key, out var value);
                return value;
            }
        }
    }
}