using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KoiAI.Pool
{
    public interface IPool { }
    public class Pool<T> : IPool where T : Object
    {
        private readonly Stack<T> _poolStacks;
        private Transform _poolStorage;
        private int _initPoolSize;
        private int _maxPoolSize;
        private T _prefabObj;
        public Pool(T prefabObj, int initPoolSize, int maxPoolSize, PoolName poolName) 
        {
            _poolStorage = PoolStorage.GetStorage(poolName);
            if(_poolStorage == null)
            {
                _poolStorage = default;
            }
            _initPoolSize = initPoolSize;
            _maxPoolSize = maxPoolSize;
            _poolStacks = new Stack<T>(_initPoolSize);
            _prefabObj = prefabObj;
            Init(prefabObj);
        }

        public void Init(T prefabObj)
        {
            for (int i = 0; i < _initPoolSize; i++)
            {
                T newObj =  Object.Instantiate(prefabObj, _poolStorage);
                SetActive(false, newObj);
                _poolStacks.Push(newObj);
            }
        }

        public void Return(T objToReturn)
        {
            // Unity's destroyed objects compare equal to null. Never keep one in the pool.
            if (!objToReturn)
            {
                return;
            }

            if (_poolStacks.Count >= _maxPoolSize)
            {
                GameObject objToDestory = ConvertToGameObject(objToReturn);
                if (objToDestory)
                {
                    Object.Destroy(objToDestory);
                }
            }
            else
            {
                SetActive(false, objToReturn);
                _poolStacks.Push(objToReturn);
            }
        }

        public IEnumerator IEReturn(T objToReturn, float delayTime)
        {
            if (!objToReturn)
            {
                yield break;
            }

            yield return new WaitForSeconds(delayTime);

            if (!objToReturn)
            {
                yield break;
            }

            Return(objToReturn);
        }

        public GameObject ConvertToGameObject(T obj)
        {
            if (obj is GameObject gameObj)
            {
                return gameObj;
            }
            else if (obj is Component component)
            {
                return component.gameObject;
            }
            return null;
        }

        public void SetActive(bool active, T obj)
        {
            GameObject gameObj = ConvertToGameObject(obj);
            if(gameObj)
            {
                gameObj.SetActive(active);
            }
        }

        /// <summary>
        /// PoolName을 사용하지 않을 경우 직접 생성 위치 지정 권장
        /// </summary>
        public T Pop()
        {
            while (_poolStacks.Count > 0)
            {
                T pooledObj = _poolStacks.Pop();
                if (!pooledObj)
                {
                    continue;
                }

                SetActive(true, pooledObj);
                return pooledObj;
            }

            T spawnedObj = Object.Instantiate(_prefabObj, _poolStorage);
            SetActive(true, spawnedObj);
            return spawnedObj;
        }

        public T[] GetAllInstanceArray()
        {
            return _poolStacks.ToArray();
        }
        public List<T> GetAllInstanceList()
        {
            return _poolStacks.ToList();
        }
    }
}
