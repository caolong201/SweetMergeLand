using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

namespace MakerStudios.AxieBound
{
    /// <summary>
    /// Simple class that acts as a bridge between Unity inspector and .Net dictionary,
    /// it will draw as a list during Unity Serialization process but in .Net and Json it will act as a Dictionary during Serialization Process
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TValue">The type of value, must be qualified to Unity serialization type in order for Unity to serialize</typeparam>
    [Serializable]
    public class KeyValueCollection<T, TValue> : ISerializable where T : KeyValueItem<TValue>
    {
        [SerializeField] List<T> _rawList;

        public int Count => rawDictionary.Count;

        Dictionary<string, TValue> _rawDictionary;
        Dictionary<string, TValue> rawDictionary
        {
            get
            {
                if (_rawDictionary == null || _rawDictionary.Count == 0)
                {
                    _rawList.ForEach(item => _rawDictionary.Add(item.key, item.value));
                }
                else if (_rawDictionary.Count != _rawList.Count)
                {
                    _rawDictionary.Clear();
                    _rawList.ForEach(item => _rawDictionary.Add(item.key, item.value));
                }


                return _rawDictionary;
            }
        }

        public TValue this[string key]
        {
            get => rawDictionary[key];
            set => rawDictionary[key] = value;
        }

        public KeyValueCollection()
        {
            _rawDictionary = new();
            _rawList = new();
        }

        public void Add(T item)
        {
            if (ContainsKey(item.key))
                return;

            _rawList.Add(item);
            rawDictionary.Add(item.key, item.value);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                if (ContainsKey(item.key))
                    continue;

                _rawList.Add(item);
                rawDictionary.Add(item.key, item.value);
            }
        }

        public void Remove(T item)
        {
            _rawList.Remove(item);
            rawDictionary.Remove(item.key);
        }

        public void RemoveAt(int index)
        {
            var item = _rawList[index];
            _rawList.RemoveAt(index);
            rawDictionary.Remove(item.key);
        }

        public void Insert(int index, T item)
        {
            _rawList.Insert(index, item);
            rawDictionary.Add(item.key, item.value);
        }

        public bool TryGetValue(string key, out TValue value)
        {
            return rawDictionary.TryGetValue(key, out value);
        }

        public bool ContainsKey(string key)
        {
            return rawDictionary.ContainsKey(key);
        }

        public void AddKey(string key, TValue value)
        {
            if (ContainsKey(key))
                return;

            var item = new KeyValueItem<TValue>(key, value);
            _rawList.Add(item as T);
            rawDictionary.Add(key, value);
        }

        public void Clear()
        {
            _rawList.Clear();
            rawDictionary.Clear();
        }

        public KeyValueCollection(SerializationInfo info, StreamingContext context)
        {
            _rawList = new();
            _rawDictionary = new();

            foreach (var serializtionItem in info)
            {
                var name = serializtionItem.Name;
                var value = (TValue)info.GetValue(name, typeof(TValue));

                if (_rawDictionary.TryAdd(name, value))
                {
                    _rawList.Add((T)new KeyValueItem<TValue>(name, value));
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var item in rawDictionary)
            {
                info.AddValue(item.Key, item.Value);
            }
        }
    }

    [Serializable]
    public class KeyValueItem<TValue> : ISerializable
    {
        [SerializeField] string _key;
        public string key => _key;

        [SerializeField] TValue _value;
        public TValue value { get => _value; set => _value = value; }

        public KeyValueItem(string key, TValue value)
        {
            _key = key;
            _value = value;
        }

        public KeyValueItem(SerializationInfo info, StreamingContext context)
        {
            value = (TValue)info.GetValue(key, typeof(TValue));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(key, value);
        }
    }

}
