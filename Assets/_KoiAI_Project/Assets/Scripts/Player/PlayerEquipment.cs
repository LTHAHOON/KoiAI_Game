using NaughtyAttributes;
using R3;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KoiAI.Player
{
    using KoiAI.Input;
    using KoiAI.Interact;
    using KoiAI.Item;
    using KoiAI.ItemProp;
    using KoiAI.UI.HUD;

    [Serializable]
    public class PlayerEquipmentValueData : PlayerFeatureValueData
    {
        [SerializeField]
        private ItemPickUpSize _itemPickUpSize;

        public ItemPickUpSize ItemPickUpSize => _itemPickUpSize;
    }

    [RequireComponent(typeof(ItemPickUpInteractable), typeof(ItemPickUpConditionBinder))]
    public class PlayerEquipment : PlayerFeature, IEquipmentProvider
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
            
            public PlayerEquipmentPoint(ItemCategory category, Transform point)
            {
                _category = category;
                _point = point;
            }
        }

        [ReadOnly]
        [SerializeField]
        private PlayerEquipmentValueData _valueData;
        [SerializeField]
        private InventorySystem _inventorySystem;
        [SerializeField]
        private WeaponInfoControl _weaponInfoControl;
        [Header("장착된 아이템")]
        [SerializeField] 
        private List<ItemData> _equipDatas = new();
        [Header("장착되지 않은 아이템 ")]
        [SerializeField] 
        private List<ItemData> _notEquipDatas = new();
        [Header("장비 저장 위치")]
        [SerializeField]
        private List<PlayerEquipmentPoint> _itemParentPoints = new();

        private ItemPickUpInteractable _itemInteractable;
        private readonly Dictionary<ItemCategory, Transform> _dicItemParentPoint = new();
        private readonly StringBuilder _sb = new();
        public override PlayerFeatureProperty FeatureProperty => PlayerFeatureProperty.Equipment;
        public override void InitAutoInEnditor()
        {
            _inventorySystem = FindAnyObjectByType<InventorySystem>();
            _weaponInfoControl = FindAnyObjectByType<WeaponInfoControl>();
        }

        public override void Init(PlayerFeatureValueData playerFeatureValueData = null, 
            PlayerFeatureExtensionData playerFeatureExtensionData = null)
        {
            PlayerSkin playerSkin = Owner.CurrentPlayerSkin;
            if (playerSkin == null)
            {
                return;
            }
            if (playerFeatureValueData is not PlayerEquipmentValueData valueData)
            {
                return;
            }
            _valueData = valueData;
            #region 아이템 생성 위치 초기화
            AddItemParent(ItemCategory.Weapon, playerSkin.WeaponPoint);
            AddItemParent(ItemCategory.Resource, playerSkin.ResoucePoint);
            #endregion
            
            #region 인스펙터 아이템 저장 위치 세팅
            for (int i = 0; i < _itemParentPoints.Count; i++)
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
                _inventorySystem.CreateAndPushItem(Owner, parent, ItemSlotType.Equipped, _equipDatas[i]);
            }

            for (int i = 0; i < notEquipDataCount; i++)
            {
                Transform parent = _dicItemParentPoint[_notEquipDatas[i].ItemPrefab.Category];
                _inventorySystem.CreateAndPushItem(Owner, parent, ItemSlotType.NotEquipped, _notEquipDatas[i]);
            }
            #endregion

            #region ItemInteractable 구독

            _itemInteractable = GetComponent<ItemPickUpInteractable>();
            _itemInteractable.OnInteract.Subscribe(itemPickUpEvent =>
            {
                PickUpItem(itemPickUpEvent.ItemData);
            });

            #endregion

            InputService.PlayerIA.Player.SelectItem_Equip.performed += OnSelectEquipItem;
            InputService.PlayerIA.Player.SelectItem_NotEquip.performed += OnSelectNotEquipItem;
            InputService.PlayerIA.Player.UseItem.performed += OnUseItem;
            if(_inventorySystem.TryGetInventorySlot(ItemSlotType.NotEquipped, out var inventorySlot))
            {
                _maxSelectNotEquipIndex = inventorySlot.SlotData.SlotCount;
            }
        }

        public override void UpdateFeature() { }

        private void PickUpItem(ItemData itemData, ItemSlotType slotType = ItemSlotType.NotEquipped)
        {
            var itemList = GetItemList(slotType);
            bool isExistEmptySlot = IsExistEmptySlot(itemList, slotType);
            if (isExistEmptySlot)
            {
                itemList.Add(itemData);
                Transform parent = _dicItemParentPoint[itemData.ItemPrefab.Category];
                _inventorySystem.CreateAndPushItem(Owner, parent, slotType, itemData);
            }
        }
    
        private List<ItemData> GetItemList(ItemSlotType itemSlotType)
        {
            switch (itemSlotType)
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

        private void OnUseItem(InputAction.CallbackContext context)
        {
            if(context.performed)
            {
                Slot selectedSlot = GetSelectedSlot(ItemSlotType.NotEquipped);
                if (!selectedSlot)
                    return;
                ItemBase item = selectedSlot.GetItem();
                if (item != null)
                {
                    item.UseItem();
                }
            }
        }

        public void SetItemCount(ItemBase item, int count)
        {
            if(!item)
            {
                return;
            }
            Slot slot = _inventorySystem.GetSlotWithItem(item);
            if (slot)
            {
                _sb.Append(count);
                slot.SetItemCountText(_sb);
                _sb.Clear();
            }
        }

        public void EquipItem(ItemBase item)
        {
            ItemData itemData = item.GetItemData();
            if(!_notEquipDatas.Contains(itemData))
            {
                return;
            }
            _notEquipDatas.Remove(item.GetItemData());
            _equipDatas.Add(item.GetItemData());
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
                int curSelectedIndex = Mathf.RoundToInt(value - 1);
                if (curSelectedIndex < 0)
                    return;
                int prevSelectedIndex = _inventorySystem.FindSelectedIndex(ItemSlotType.Equipped);
                _inventorySystem.DeSelectItemSlot(ItemSlotType.Equipped, prevSelectedIndex);
                _inventorySystem.SelectItemSlot(ItemSlotType.Equipped, curSelectedIndex);

                _inventorySystem.TryGetInventorySlot(ItemSlotType.Equipped, out var inventorySlot);
                Slot slot = _inventorySystem.FindSlotWithIndex(inventorySlot, curSelectedIndex);
                if(!slot)
                {
                    return;
                }
                if (slot.IsEmpty())
                {
                    SetWeaponInfo(0, 0);
                }
            }
        }

        public void PushItemInSlot(ItemBase item, ItemSlotType slotTypeToPush)
        {
            Slot curSlot = _inventorySystem.GetSlotWithItem(item);
            if (curSlot == null)
                return;
            _inventorySystem.PushItem(curSlot, slotTypeToPush);
        }

        public void CreateAndPushItemInSlot(ItemSlotType slotTypeToPush, ItemData itemData)
        {
            if (itemData == null)
                return;
            var itemList = GetItemList(slotTypeToPush);
            if(itemList == null)
                return;
        
            itemList.Add(itemData);
            Transform itemParent = GetItemParent(itemData.ItemPrefab.Category);
            _inventorySystem.CreateAndPushItem(Owner, itemParent, slotTypeToPush, itemData);
        }
    
        public void RemoveItemInSlot(ItemBase item)
        {
            Slot slot = _inventorySystem.GetSlotWithItem(item);
            ItemSlotType itemSlotType = item.GetCurrentSlotType();

            var itemDatas = GetItemList(itemSlotType);
            ItemData itemData = item.GetItemData();
            if(itemDatas == null || itemData == null || slot == null)
                return;
            itemDatas.Remove(itemData);
            _inventorySystem.RemoveItem(slot, itemSlotType);
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

        
        private void AddItemParent(ItemCategory category, Transform parent)
        {
            PlayerEquipmentPoint point = new PlayerEquipmentPoint(category, parent);
            _itemParentPoints.Add(point);
            _dicItemParentPoint.TryAdd(point.Category, point.Point);
        }
        
        public Transform GetItemParent(ItemCategory category)
        {
            if(_dicItemParentPoint.TryGetValue(category, out Transform parent))
            {
                return parent;
            }
            return null;
        }

        public bool IsExistSameID(ItemBase item, ItemSlotType slotTypeToFind)
        {
            var itemList =  GetItemList(slotTypeToFind);
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].ItemId == item.GetItemData().ItemId)
                {
                    return true;
                }
            }
            return false;
        }
    
        public bool IsExistEmptySlot(List<ItemData> itemList, ItemSlotType slotType)
        {
            _inventorySystem.TryGetInventorySlot(slotType, out var inventorySlot);
            int curExistedCount = itemList.Count;
            int maxSlotCount = inventorySlot.SlotData.SlotCount;
            if (curExistedCount < maxSlotCount)
            {
                return true;
            }
            return false;
        }

        public void RefreshItemPickUpCondition(ItemPickUpCondition currentConditionData, ItemPickUpCondition compareCondition)
        {
            int remainingSlotCount = GetRemainingSlotCount();
            int maxSlotCount = GetMaxSlotCount();

            var compareSlotCountCondtiion = compareCondition.slotCountCompareCondition;
            compareSlotCountCondtiion.SetCompareValue(maxSlotCount);
            compareCondition.slotCountCompareCondition = compareSlotCountCondtiion;

            var conditionSlotCountData = currentConditionData.slotCountCompareCondition;
            conditionSlotCountData.SetCompareValue(remainingSlotCount);
            currentConditionData.slotCountCompareCondition = conditionSlotCountData;

            var conditionItemSizeData = currentConditionData.itemSizeCompareCondition;
            conditionItemSizeData.SetCompareValue(ItemPickUpSize);
            currentConditionData.itemSizeCompareCondition = conditionItemSizeData;
        }

        public int GetMaxSlotCount()
        {
            _inventorySystem.TryGetInventorySlot(ItemSlotType.NotEquipped, out var inventorySlot);
            int maxSlotCount = inventorySlot.SlotData.SlotCount;
            return maxSlotCount;
        }

        public int GetRemainingSlotCount() => _notEquipDatas.Count;


        public ItemPickUpSize ItemPickUpSize => _valueData.ItemPickUpSize;
    }
}
