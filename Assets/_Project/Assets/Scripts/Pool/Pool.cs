
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        if (_poolStacks.Count > _maxPoolSize)
        {
            GameObject objToDestory = ConvertToGameObject(objToReturn);
            Object.Destroy(objToDestory);
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
            yield return null;
        yield return new WaitForSeconds(delayTime);
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
    public T Pop()
    {
        if (_poolStacks.Count <= 0)
        {
            T newObj = Object.Instantiate(_prefabObj, _poolStorage);
            _poolStacks.Push(newObj);
        }
        T obj = _poolStacks.Pop();
        SetActive(true, obj);
        return obj;
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

