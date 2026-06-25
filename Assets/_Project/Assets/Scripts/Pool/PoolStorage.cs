using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PoolName
{
    Projectile,
    WayPoint,
    AudioSource,
}
[Serializable]
public struct PoolStorageData
{
    [Header("중복 허용 X")]
    [SerializeField]
    private PoolName poolName;
    [SerializeField]
    private Transform _storage;

    public Transform Storage => _storage;
    public PoolName PoolName => poolName;   
}
public class PoolStorage : MonoBehaviour
{
    [SerializeField]
    private List<PoolStorageData> _poolStorageDatas;
    private static Dictionary<int, PoolStorageData> _dicStorageDatas;
    private void Awake()
    {
        Init();
    }
    private void OnDestroy()
    {
        _dicStorageDatas.Clear();
    }

    private void Init()
    {
        _dicStorageDatas = new();
        for (int i = 0; i < _poolStorageDatas.Count; i++)
        {
            PoolStorageData storageData = _poolStorageDatas[i];
            int enumID = (int)storageData.PoolName;
            Transform storage = storageData.Storage;
            _dicStorageDatas.TryAdd(enumID, storageData);
        }
    }

    public static Transform GetStorage(PoolName storageName)
    {
        bool bGet = _dicStorageDatas.TryGetValue((int)storageName, out PoolStorageData poolStorageData);
        if(bGet)
        {
            return poolStorageData.Storage;
        }
        return null;
    }
}
