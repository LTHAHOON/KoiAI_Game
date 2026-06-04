using System;
using System.Collections.Generic;
using UnityEngine;


public class PlayerEquipment : MonoBehaviour
{
    [SerializeField]
    private InventorySystem _inventorySystem;
    [SerializeField] 
    private List<ItemBase>  _equipDatas;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        for (int i = 0; i < _equipDatas.Count; i++)
        {
            _inventorySystem.PushItem(_equipDatas[i]);
        }
    }
}
