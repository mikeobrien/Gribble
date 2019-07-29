using System.Collections;
using System.Collections.Generic;

namespace Gribble.Collections
{
    public abstract class DictionaryBase<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        protected DictionaryBase()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        protected DictionaryBase(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public virtual TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public virtual ICollection<TKey> Keys => _dictionary.Keys;
        public virtual ICollection<TValue> Values => _dictionary.Values;
        public virtual int Count => _dictionary.Count;
        public virtual bool IsReadOnly  => _dictionary.IsReadOnly;

        public virtual void Add(KeyValuePair<TKey, TValue> item) => _dictionary.Add(item);
        public virtual void Clear() => _dictionary.Clear();
        public virtual bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _dictionary
            .CopyTo(array, arrayIndex);
        public virtual bool Remove(KeyValuePair<TKey, TValue> item) => _dictionary.Remove(item);
        public virtual bool ContainsKey(TKey key)  => _dictionary.ContainsKey(key);
        public virtual void Add(TKey key, TValue value) => _dictionary.Add(key, value);
        public virtual bool Remove(TKey key) => _dictionary.Remove(key);
        public virtual bool TryGetValue(TKey key, out TValue value) => 
            _dictionary.TryGetValue(key, out value);
        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();
    }
}