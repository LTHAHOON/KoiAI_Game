using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [Header("장착 가능한 아이템 (무기/갑옷/..)")]
    [SerializeField] 
    private List<ItemData>  _equipDatas;
    [Header("장착 불가능 아이템 (총알/아티팩트/..)")]
    [SerializeField] 
    private List<ItemData>  _notEquipDatas;
    [Header("장비 저장 위치")]
    [SerializeField]
    private PlayerEquipmentPoint[] _itemParentPoints;

    private readonly Dictionary<ItemCategory, Transform> _dicItemParentPoint = new();

    public override PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.Equipment;

    public override void Init(PlayerInputAction playerIA)
    {
        #region 인스펙터 아이템 저장 위치 세팅
        for (int i = 0; i < _itemParentPoints.Length; i++)
        {
            _dicItemParentPoint.TryAdd(_itemParentPoints[i].Category, _itemParentPoints[i].Point);
        }
        #endregion

        #region 아이템 생성
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
        #endregion
    }
    
    public override void UpdateFeature() { }

    private List<ItemData> GetItemList(ItemData itemData)
    {
        switch (itemData.Type)
        {
            case ItemSlotType.Equipped:
            {
                return _equipDatas;
            }
            case ItemSlotType.NotEquipped:
            {
                return _notEquipDatas;
            }
            default:
                return null;
        }
    }
    
    public Slot AddItemInSlot(ItemData itemData)
    {
        if (itemData == null)
            return null;
        var itemList = GetItemList(itemData);
        if(itemList == null)
            return null;
        
        itemList.Add(itemData);
        Transform itemParent = GetItemParent(itemData.ItemPrefab.Category);
        return _inventorySystem.CreateAndPushItem(Owner, itemParent, itemData);
    }
    
    public void RemoveItemInSlot(Slot targetSlot, ItemData itemData)
    {
        if (itemData == null)
            return;
        var itemDatas = GetItemList(itemData);
        if(itemDatas == null)
            return;
        itemDatas.Remove(itemData);
        _inventorySystem.RemoveItem(targetSlot, itemData.Type);
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
