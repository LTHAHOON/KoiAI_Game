using System;
using System.Collections.Generic;
using UnityEngine;

namespace KoiAI.Pool
{
    [Serializable]
    public struct PoolSize
    {
        [SerializeField]
        private int _initPoolSize;
        [SerializeField]
        private int _maxPoolSize;
        public int InitPoolSize => _initPoolSize;
        public int MaxPoolSize => _maxPoolSize; 
        public PoolSize(int initBulletPoolSize = 5, int maxPoolSize = 10)
        {
            _initPoolSize = initBulletPoolSize;
            _maxPoolSize = maxPoolSize;
        }
    }

    public class PoolManager : MonoBehaviour
    {
        private readonly Dictionary<ulong, IPool> _dicPool = new();
        public static PoolManager Instance;
        private void Awake()
        {
            Instance = this;
        }

        public void AddPool<T>(ulong id, T prefab, PoolSize poolSize, PoolName poolName) where T : UnityEngine.Object
        {
            if (_dicPool.ContainsKey(id))
                return;
            Pool<T> pool = new(prefab, poolSize.InitPoolSize, poolSize.MaxPoolSize, poolName);
            _dicPool.Add(id, pool);
        }

        public bool TryGetPool<T>(ulong prefabID, out Pool<T> pool) where T : UnityEngine.Object
        {
            bool bGet = _dicPool.TryGetValue(prefabID, out IPool poolObj);
            if (poolObj is Pool<T> tPool)
            {
                pool = tPool;
            }
            else
            {
                pool = default;
            }
            return bGet;
        }

        public void ReturnDelay<T>(Pool<T> pool, T obj, float delayTime) where T : UnityEngine.Object
        {
            if (pool == null)
                return;
            StartCoroutine(pool.IEReturn(obj, delayTime));
        }
    }
}