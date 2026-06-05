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
    private List<ItemBase>  _equipDatas;
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
        for (int i = 0; i < _equipDatas.Count; i++)
        {
            Transform parent = _dicItemParentPoint[_equipDatas[i].Category];
            _inventorySystem.CreateAndPushItem(Owner, parent, _equipDatas[i]);
        }
    }
    public override void UpdateFeature() { }

    public Transform GetItemParent(ItemCategory category)
    {
        if(_dicItemParentPoint.TryGetValue(category, out Transform parent))
        {
            return parent;
        }
        return null;
    }
}
