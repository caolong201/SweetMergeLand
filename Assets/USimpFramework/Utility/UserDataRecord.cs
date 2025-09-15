using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace USimpFramework.Utility
{
    public interface IUserData
    {

    }

    public abstract class UserDataRecord<TData> where TData : IUserData
    {

        public string dataKey { get; private set; }

        public TData data { get; protected set; }

        public UserDataRecord(string dataKey)
        {
            this.dataKey = dataKey;
        }

        public abstract void SaveData(Action onSuccess, Action<string> onError);

        public abstract Task<bool> SaveDataAsync();

        public abstract void LoadData(Action onSuccess, Action<string> onError);

        public abstract Task<bool> LoadDataAsync();

        public abstract void FromJson(string json);

        public abstract void SetData(TData newData);

    }

    public abstract class UserDataRecordCollection<TData> where TData : IUserData
    {
        public string dataKey { get; private set; }

        public List<TData> dataCollection { get; protected set; }

        public UserDataRecordCollection(string dataKey)
        {
            this.dataKey = dataKey;
            dataCollection = new();
        }

        public abstract void SaveData(Action onSuccess, Action<string> onError);

        public abstract Task<bool> SaveDataAsync();

        public abstract void LoadData(Action onSuccess, Action<string> onError);

        public abstract Task<bool> LoadDataAsync();

        public abstract void FromJson(string json);

        public abstract void SetData(IEnumerable<TData> newData);

    }
}


