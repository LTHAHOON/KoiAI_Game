using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


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
    private WeaponInfoControl _weaponInfoControl;
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
    private readonly StringBuilder _sb = new();
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

        playerIA.Player.SelectItem_Equip.performed += OnSelectEquipItem;
        playerIA.Player.SelectItem_NotEquip.performed += OnSelectNotEquipItem;
        if(_inventorySystem.TryGetInventorySlot(ItemSlotType.NotEquipped, out var inventorySlot))
        {
            _maxSelectNotEquipIndex = inventorySlot.SlotData.SlotCount;
        }
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

    private int _curSelectNotEquipIndex = 0;
    private int _maxSelectNotEquipIndex = 0;
    private void OnSelectNotEquipItem(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            float value = context.ReadValue<float>();
            int iValue = Mathf.RoundToInt(value);
            _curSelectNotEquipIndex += iValue;
            if(_curSelectNotEquipIndex < 0)
            {
                _curSelectNotEquipIndex = _maxSelectNotEquipIndex - 1;
            }
            _curSelectNotEquipIndex %= _maxSelectNotEquipIndex;

            int prevSelectedIndex = _inventorySystem.FindSelectedIndex(ItemSlotType.NotEquipped);
            _inventorySystem.DeSelectItemSlot(ItemSlotType.NotEquipped, prevSelectedIndex);
            _inventorySystem.SelectItemSlot(ItemSlotType.NotEquipped, _curSelectNotEquipIndex);
        }
    }

    private void OnSelectEquipItem(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float value = context.ReadValue<float>();
            int index = Mathf.RoundToInt(value - 1);
            if (index < 0)
                return;
            int prevSelectedIndex = _inventorySystem.FindSelectedIndex(ItemSlotType.Equipped);
            _inventorySystem.DeSelectItemSlot(ItemSlotType.Equipped, prevSelectedIndex);
            _inventorySystem.SelectItemSlot(ItemSlotType.Equipped, index);
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

    public void SetWeaponInfo(int curCount, int remainingCount)
    {
        if (!_weaponInfoControl || _sb == null)
            return;
        _sb.Append(curCount);
        _weaponInfoControl.SetCurCountText(_sb);
        _sb.Clear();
        _sb.Append(remainingCount);
        _weaponInfoControl.SetRemainingCountText(_sb);
        _sb.Clear();
    }

    public Slot GetSelectedSlot(ItemSlotType itemSlotType)
    {
        int selectedIndex = _inventorySystem.FindSelectedIndex(itemSlotType);
        bool bGet = _inventorySystem.TryGetInventorySlot(itemSlotType, out var inventorySlot);
        if (!bGet)
            return null;
        Slot selectedSlot = _inventorySystem.FindSlotWithIndex(inventorySlot, selectedIndex);
        return selectedSlot;
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
