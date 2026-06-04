using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ItemCategory
{
    Weapon,
    Resource
}

public class InventorySystem : MonoBehaviour
{
    [Serializable]
    public struct InventorySlot
    {
        [SerializeField]
        private Transform _slotParent;
        [SerializeField]
        private SlotData _slotData;
        public readonly Transform SlotParent => _slotParent;
        public readonly SlotData SlotData => _slotData;

        public void Init(Dictionary<int, Transform> dicSlotParent)
        {
            if (!_slotData || !_slotParent)
                return;
            for (int i = 0; i < _slotData.SlotCount; i++)
            {
                Instantiate(_slotData.SlotPrefab, _slotParent);
            }
            dicSlotParent.Add((int)_slotData.SlotCategory, _slotParent);
        }
    }

    [SerializeField]
    private InventorySlot[] _inventorySlots;
    [SerializeField]
    private Image _itemImagePrefab;
    private readonly Dictionary<int, Transform> _dicSlotParent = new();
    private readonly Dictionary<ulong, GameObject> _dicEquippedItem = new();
    private void Awake()
    {
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            _inventorySlots[i].Init(_dicSlotParent);
        }
    }

    private bool TryGetSlotParent(out Transform parent, ItemCategory category)
    {
        if (_dicSlotParent == null)
        {
            parent = default;
            return false;
        }
        bool bGet = _dicSlotParent.TryGetValue((int)category, out parent);
        return bGet;
    }

    public void PushItem<T>(IItem<T> item) where T : MonoBehaviour, IItem<MonoBehaviour>
    {
        if (!TryGetSlotParent(out Transform parent, item.Category))
            return;
         Image newItemImage = Instantiate(_itemImagePrefab, parent);
        newItemImage.sprite = item.Texture;
    }
}
