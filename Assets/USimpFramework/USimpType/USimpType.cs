using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace USimpFramework.USimpType
{
    public class IgnoreSaveDataToken
    {
        public bool isIgnore { get; set; }
    }

    //Critical todo: Make it works with string and json
    /// <summary>The generic class type that provides feature like observable, save data, load data, encryption</summary>
    [Serializable]
    public abstract class USimpType<T>
    {
        public event Action<T> onValueChanged;

        public USimpType(T value, string dataKey)
        {
            _value = value;
            this.dataKey = dataKey;
        }

        public USimpType(string dataKey)
        {
            this.dataKey = dataKey;
            _value = Activator.CreateInstance<T>();
        }

        public string dataKey { get; private set; }

        protected IgnoreSaveDataToken ignoreSaveDataToken;

        T oldValue;

        [SerializeField] T _value;
        public T value
        {
            get => _value;
            set
            {
                if (_value.Equals(value))
                    return;

                oldValue = _value;
                _value = value;
                onValueChanged?.Invoke(oldValue);
            }
        }

        public void SetIgnoreSaveDataToken(IgnoreSaveDataToken ignoreSaveDataToken)
        {
            this.ignoreSaveDataToken = ignoreSaveDataToken;
        }

        public void SetValueWithoutNotify(T value)
        {
            _value = value;
            oldValue = value;
        }


        public void Notify()
        {
            onValueChanged?.Invoke(oldValue);
        }


        public static implicit operator T(USimpType<T> type)
        {
            return type.value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public abstract void SaveData(Action onSuccess, Action<string> onError);

        public abstract Task<bool> SaveDataAsync();

        public abstract void LoadData(Action onSuccess, Action<string> onError);

        public abstract Task<bool> LoadDataAsync();

        public abstract void FromJson(string json);


    }

    [Serializable]
    public abstract class USimpTypeCollection<T>
    {
        [JsonIgnore] public Action<T> onAdded;
        [JsonIgnore] public Action<IEnumerable<T>> onRangeAdded;
        [JsonIgnore] public Action onRemovedAll;
        [JsonIgnore] public Action<T> onRemoved;

        public T this[int i]
        {
            get => dataCollection[i];
            set => dataCollection[i] = value;
        }

        public void Add(T item)
        {
            dataCollection.Add(item);
            onAdded?.Invoke(item);
        }

        public void Remove(T item)
        {
            dataCollection.Remove(item);
            onRemoved?.Invoke(item);
        }

        public void RemoveAt(int index)
        {
            var removedItem = dataCollection[index];
            dataCollection.RemoveAt(index);
            onRemoved?.Invoke(removedItem);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            dataCollection.AddRange(collection);
            onRangeAdded?.Invoke(collection);
        }

        public void RemoveAll(Predicate<T> match)
        {
            dataCollection.RemoveAll(match);
            onRemovedAll?.Invoke();
        }

        public T Find(Predicate<T> match)
        {
            return dataCollection.Find(match);
        }

        public int Count => dataCollection.Count;

        public string dataKey { get; private set; }

        public List<T> dataCollection { get; protected set; }

        protected IgnoreSaveDataToken ignoreSaveDataToken;


        public void SetIgnoreSaveDataToken(IgnoreSaveDataToken ignoreSaveDataToken)
        {
            this.ignoreSaveDataToken = ignoreSaveDataToken;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return dataCollection.GetEnumerator();
        }

        public USimpTypeCollection(string dataKey)
        {

            this.dataKey = dataKey;
            dataCollection = new();
        }

        public abstract void SaveData(Action onSuccess, Action<string> onError);

        public abstract Task<bool> SaveDataAsync();

        /// <summary>Load data does not trigger the on value changed event  </summary>
        public abstract void LoadData(Action onSuccess = null, Action<string> onError = null);

        /// <summary>Load data does not trigger the on value changed event  </summary>
        public abstract Task<bool> LoadDataAsync();

        public abstract void FromJson(string json);

        public abstract void SetData(IEnumerable<T> newData);
    }


    [Serializable]
    public class PlayerPrefsType<T> : USimpType<T>
    {
        public PlayerPrefsType(string dataKey) : base(dataKey)
        {

        }

        public PlayerPrefsType(T value, string dataKey) : base(value, dataKey) { }

        public override void FromJson(string json)
        {
            value = JsonConvert.DeserializeObject<T>(json);
        }

        public override void LoadData(Action onSuccess = null, Action<string> onError = null)
        {
            if (!PlayerPrefs.HasKey(dataKey))
                return;

            var type = typeof(T);
            string rawDataStr;

            if (type == typeof(int))
            {
                rawDataStr = PlayerPrefs.GetInt(dataKey).ToString();
            }
            else if (type == typeof(float))
            {
                rawDataStr = PlayerPrefs.GetFloat(dataKey).ToString();
            }
            else //string or json
            {
                rawDataStr = PlayerPrefs.GetString(dataKey);
            }

            try
            {
                SetValueWithoutNotify(JsonConvert.DeserializeObject<T>(rawDataStr));
            }
            catch (Exception error)
            {
                //If can not deseialize object from string, set directly that to value string
                Debug.LogError($"Failed to deserialize {dataKey}, error {error.Message}");
                rawDataStr = $"\"{rawDataStr}\"";
                SetValueWithoutNotify(JsonConvert.DeserializeObject<T>(rawDataStr));

                onError?.Invoke(error.Message);
                return;
            }


            onSuccess?.Invoke();
        }

        public override Task<bool> LoadDataAsync()
        {
            throw new NotImplementedException();
        }

        public override void SaveData(Action onSuccess = null, Action<string> onError = null)
        {
            if (ignoreSaveDataToken != null && ignoreSaveDataToken.isIgnore)
                return;

#if ENABLE_USIMPTYPE_DEBUG_LOG
            Debug.Log($"{dataKey} Save data!");
#endif
            if (value == null)
            {
                string err = $"Save data {dataKey} failed!. Invalid value!";
                Debug.LogError(err);
                onError?.Invoke(err);
                return;
            }

            var type = typeof(T);

            if (type == typeof(int))
            {
                PlayerPrefs.SetInt(dataKey, int.Parse(value.ToString()));
            }
            else if (type == typeof(float))
            {
                PlayerPrefs.SetFloat(dataKey, float.Parse(value.ToString()));
            }
            else if (type == typeof(string))
                PlayerPrefs.SetString(dataKey, value.ToString());
            else
                PlayerPrefs.SetString(dataKey, JsonConvert.SerializeObject(value));
            onSuccess?.Invoke();
        }

        public override Task<bool> SaveDataAsync()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class PlayerPrefsTypeCollection<T> : USimpTypeCollection<T>
    {
        public PlayerPrefsTypeCollection(string dataKey) : base(dataKey)
        {

        }

        public override void FromJson(string json)
        {
            dataCollection = JsonConvert.DeserializeObject<List<T>>(json);
        }

        public override void LoadData(Action onSuccess = null, Action<string> onError = null)
        {
            //Debug.Log($"{dataKey} load data!");

            var json = PlayerPrefs.GetString(dataKey);
            if (string.IsNullOrEmpty(json))
                return;

            try
            {
                dataCollection = JsonConvert.DeserializeObject<List<T>>(json);
            }
            catch (Exception error)
            {

                Debug.LogError($"{dataKey} load data failed!. Error {error.Message}");
                onError?.Invoke(error.Message);
                return;
            }

            onSuccess?.Invoke();
        }

        public override Task<bool> LoadDataAsync()
        {
            throw new NotImplementedException();
        }

        public override void SaveData(Action onSuccess = null, Action<string> onError = null)
        {
            if (ignoreSaveDataToken != null && ignoreSaveDataToken.isIgnore)
                return;

#if ENABLE_USIMPTYPE_DEBUG_LOG
            Debug.Log($"{dataKey} save data!");
#endif

            if (dataCollection == null)
            {
                string err = $"Save data {dataKey} failed!. Invalid collection!";
                Debug.Log(err);
                onError?.Invoke(err);
                return;
            }

            try
            {
                PlayerPrefs.SetString(dataKey, JsonConvert.SerializeObject(dataCollection));
            }
            catch (Exception e)
            {
                Debug.LogError($"Save data {dataKey} failed!.Error {e.Message}");
                onError?.Invoke(e.Message);
                return;
            }

            onSuccess?.Invoke();
        }

        public override Task<bool> SaveDataAsync()
        {
            throw new NotImplementedException();
        }

        public override void SetData(IEnumerable<T> newData)
        {
            dataCollection.Clear();
            dataCollection.AddRange(newData);
        }
    }
}
