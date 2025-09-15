using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USimpFramework.Utility
{
    public class SimpleSingleton<T> : MonoBehaviour where T : Component
    {
        /*static T _Instance = null;

        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<T>();

                    if (_Instance == null)
                    {
                        var gameObj = new GameObject(nameof(T));
                        _Instance = gameObj.AddComponent<T>();
                    }
                }

                return _Instance;
            }
        }*/

        public static T Instance { get; private set; } = null;

        [Tooltip("Dont destroy When loading new scene")]
        [SerializeField] bool isPersistent;

        protected virtual void Awake()
        {
            if (Instance == null)
                Instance = this as T;
            else if (Instance != this)
                DestroyImmediate(gameObject);

            if (this != null && isPersistent)
                DontDestroyOnLoad(gameObject);
        }
    }
}
