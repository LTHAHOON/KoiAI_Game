using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;


public class PlayerEquipment : PlayerFeature
{
    [Serializable]
    private struct PlayerEquipmentPoint
    {
        #region 특성에 따른 장비 저장 위치정보
        [SerializeField]
        private ItemCategory _category;
        [SerializeField]
        private Transform _point;
        public readonly ItemCategory Category => _category;
        public readonly Transform Point => _point;
        #endregion
    }

    [SerializeField]
    private InventorySystem _inventorySystem;
    [SerializeField] 
    private List<ItemData>  _equipDatas;
    [SerializeField] 
    private List<ItemData>  _notEquipDatas;
    [Header("장비 저장 위치")]
    [SerializeField]
    private PlayerEquipmentPoint[] _itemParentPoints;

    private readonly Dictionary<ItemCategory, Transform> _dicItemParentPoint = new();

    public override PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.Equipment;

    public override void Init(PlayerInputAction playerIA)
    {
        for (int i = 0; i < _itemParentPoints.Length; i++)
        {
            _dicItemParentPoint.TryAdd(_itemParentPoints[i].Category, _itemParentPoints[i].Point);
        }
        int equipDataCount = _equipDatas.Count;
        int notEquipDataCount = _notEquipDatas.Count;
        for (int i = 0; i < equipDataCount; i++)
        {
            Transform parent = _dicItemParentPoint[_equipDatas[i].ItemPrefab.Category];
            _inventorySystem.CreateAndPushItem(Owner, parent, _equipDatas[i]);
        }


        for (int i = 0; i < notEquipDataCount; i++)
        {
            Transform parent = _dicItemParentPoint[_notEquipDatas[i].ItemPrefab.Category];
            _inventorySystem.CreateAndPushItem(Owner, parent, _notEquipDatas[i]);
        }
    }
    public override void UpdateFeature() { }

    public Slot AddItemInSlot(ItemData itemData)
    {
        if (_notEquipDatas.Contains(itemData))
            return null; 
        _notEquipDatas.Add(itemData);
        return _inventorySystem.CreateAndPushItem(Owner, GetItemParent(itemData.ItemPrefab.Category), itemData);
    }
    
    public Transform GetItemParent(ItemCategory category)
    {
        if(_dicItemParentPoint.TryGetValue(category, out Transform parent))
        {
            return parent;
        }
        return null;
    }
}
