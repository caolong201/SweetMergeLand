using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USimpFramework.Utility
{
    /// <summary>Simple implementation of pooling object,each pool will have a unique name to specify which object is in the pool, 
    /// so you MUST NOT change the name of pooled item in runtime, other wise the PoolManager will not be able to find the pool contain that item, 
    /// Works for generic type of component or GameObject
    /// </summary>
    public class SimpleObjectPool
    {
        static readonly string POOL_NAME_SCHEMA = "{0} Pool";

        struct PoolInfo
        {
            public Transform container;//To child the item in unity hierachy
            public Queue<Object> pool;
        }

        static readonly Dictionary<string, PoolInfo> poolDic = new();

        public static T Spawn<T>(T component, Transform parent = null, bool activeOnSpawned = true) where T : Component
        {
            if (component == null)
                return null;

            var poolName = string.Format(POOL_NAME_SCHEMA, component.gameObject.name);
            if (poolDic.TryGetValue(poolName, out var info)) //Have existing pool
            {
                T pooledItem;
                if (info.pool.Count > 0)
                {
                    pooledItem = info.pool.Dequeue() as T;
                    pooledItem.transform.SetParent(parent);
                }
                else
                {
                    pooledItem = GameObject.Instantiate<T>(component, parent);
                }

                if (pooledItem.name != component.gameObject.name)
                    pooledItem.name = component.gameObject.name;

                if (activeOnSpawned)
                    pooledItem.gameObject.SetActive(true);
             
                return pooledItem;
            }
            else //Not having any pool
            {
                var poolInfo = new PoolInfo()
                {
                    container = new GameObject(poolName).transform,
                    pool = new()
                };

                var newItem = GameObject.Instantiate<T>(component, parent);
                newItem.name = component.gameObject.name;
                poolDic.Add(poolName, poolInfo);

                if(activeOnSpawned)
                    newItem.gameObject.SetActive(true);

                return newItem;
            }
        }

        public static void Despawn<T>(T item, bool deactiveOnDespawned = true, bool moveToPoolContainer = true) where T : Component
        {
            if (item == null)
                return;

            var poolName = string.Format(POOL_NAME_SCHEMA, item.name);
            if (!poolDic.TryGetValue(poolName, out var info))
            {
                Debug.Log($"{item.gameObject.name} is not in any pool, you should use Despawn only for object that was in pool before! e. Or maybe you has change the gameobject name at runtime");

                //If this pool has been destroyed, But the object in the pool is still active and we can destroy the pool (due to Dont destroy on load), the it will throw this warning
                //So we need to create a new pool and at the pooled object to it?
                info = new PoolInfo()
                {
                    container = new GameObject(poolName).transform,
                    pool = new()
                };
                poolDic.Add(poolName, info);
            }

            if (!info.pool.Contains(item))
                info.pool.Enqueue(item);

            if (moveToPoolContainer)
                item.transform.SetParent(info.container);

            if (deactiveOnDespawned)
                item.gameObject.SetActive(false);

        }

        public static GameObject Spawn(GameObject item, Transform parent = null, bool activeOnSpawned = true)
        {
            if (item == null)
                return null;

            var poolName = string.Format(POOL_NAME_SCHEMA, item.name);
            if (poolDic.TryGetValue(poolName, out var info))
            {

                GameObject pooledItem;
                if (info.pool.Count > 0)
                {
                    pooledItem = info.pool.Dequeue() as GameObject;
                    pooledItem.transform.SetParent(parent);
                }
                else
                {
                    pooledItem = GameObject.Instantiate(item, parent);
                }

                if (pooledItem.name != item.name)
                    pooledItem.name = item.name;

                if (activeOnSpawned)
                    pooledItem.SetActive(true);

                return pooledItem;
            }
            else
            {
                var poolInfo = new PoolInfo()
                {
                    container = new GameObject(poolName).transform,
                    pool = new()
                };

                var newItem = GameObject.Instantiate(item, parent);
                newItem.name = item.name;

                if (activeOnSpawned)
                    newItem.SetActive(true);

                poolDic.Add(poolName, poolInfo);
                return newItem;

            }
        }

        public static void Despawn(GameObject item, bool deactiveOnSpawn = true, bool moveToPoolContainer = true)
        {
            var poolName = string.Format(POOL_NAME_SCHEMA, item.name);
            if (!poolDic.TryGetValue(poolName, out var info))
            {
                Debug.Log($"{item.name} is not in any pool, you should use Despawn only for object that was in pool before! e. Or maybe you has change the gameobject name at runtime");

                //If this pool has been destroyed, But the object in the pool is still active and we can destroy the pool (due to Dont destroy on load), the it will throw this warning
                //So we need to create a new pool and at the pooled object to it?
                info = new PoolInfo()
                {
                    container = new GameObject(poolName).transform,
                    pool = new()
                };
                poolDic.Add(poolName, info);
            }

            //Check for enqueue duplicated object
            if (!info.pool.Contains(item))
                info.pool.Enqueue(item);

            if (moveToPoolContainer)
                item.transform.SetParent(info.container);

            if (deactiveOnSpawn)
                item.SetActive(false);
        }


        public static void ClearPool(string pooledItemName)
        {
            var poolName = string.Format(POOL_NAME_SCHEMA, pooledItemName);
            if (poolDic.TryGetValue(poolName, out var info))
            {
                GameObject.Destroy(info.container.gameObject);
                info.pool.Clear();
            }
            else
                throw new UnityException($"Unnable to find pool, pool name should be {pooledItemName} Pool");
        }

        public static void ClearAll()
        {
            foreach (var info in poolDic.Values)
            {
                foreach (var gameObj in info.pool)
                {
                    GameObject.Destroy(gameObj);
                }

                if (info.container != null)
                {
                    GameObject.Destroy(info.container.gameObject);
                }

                info.pool.Clear();
            }

            poolDic.Clear();
        }
    }
}
