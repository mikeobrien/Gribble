using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gribble.Mapping;

namespace Gribble
{
    public class EntityDictionary : IDictionary<string, object>
    {
        private object _entity;
        private readonly IEntityMapping _mapping;

        public EntityDictionary(IEntityMapping mapping)
        {
            _mapping = mapping;
        }

        public void Init(object entity, IDictionary<string, object> values)
        {
            _entity = entity;
            DynamicValues = values;
        }

        public IDictionary<string, object> DynamicValues { get; private set; }

        public bool IsReadOnly => false;
        public int Count => DynamicValues.Count + _mapping.StaticProperty.Mapping.Count;
        public void Clear() => DynamicValues.Clear();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public object this[string key]
        {
            get => _mapping.StaticProperty.StaticDynamicMapping.ContainsKey(key)
                ? _mapping.StaticProperty.GetProperty(key).GetValue(_entity)
                : DynamicValues[key];
            set
            {
                if (_mapping.StaticProperty.HasColumnMapping(key))
                    _mapping.StaticProperty.GetProperty(key).SetValue(_entity, value);
                else DynamicValues[key] = value;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => 
            CreateDictionary().GetEnumerator();

        public void Add(string key, object value) => this[key] = value;
        public void Add(KeyValuePair<string, object> item) => Add(item.Key, item.Value);

        public bool Contains(KeyValuePair<string, object> item) => ContainsKey(item.Key);
        
        public bool ContainsKey(string key) => DynamicValues.ContainsKey(key) || 
            _mapping.StaticProperty.StaticDynamicMapping.ContainsKey(key);

        public bool Remove(KeyValuePair<string, object> item) => Remove(item.Key);
        public bool Remove(string key)
        {
            if (!DynamicValues.ContainsKey(key)) return false;
            DynamicValues.Remove(key);
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            if (!_mapping.StaticProperty.StaticDynamicMapping.ContainsKey(key))
                return DynamicValues.TryGetValue(key, out value);
            value = _mapping.StaticProperty.GetProperty(key).GetValue(_entity);
            return true;
        }

        public ICollection<string> Keys => DynamicValues.Keys.Union(
            _mapping.StaticProperty.StaticDynamicMapping.Select(x => x.Key)).ToList();

        public ICollection<object> Values => DynamicValues.Values.Union(
            _mapping.StaticProperty.Mapping.Select(x => x.Property.GetValue(_entity))).ToList();

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
            CreateDictionary().ToArray().CopyTo(array, arrayIndex);

        private IEnumerable<KeyValuePair<string, object>> CreateDictionary() =>
            DynamicValues.Union(_mapping.StaticProperty.StaticDynamicMapping
                .ToDictionary(y => y.Key, y => y.Value.GetValue(_entity)));
    }
}