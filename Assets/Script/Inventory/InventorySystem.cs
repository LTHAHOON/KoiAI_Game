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

        public void Init(Dictionary<int, Queue<Transform>> dicSlotTransform)
        {
            if (!_slotData || !_slotParent)
                return;
            if (!dicSlotTransform.ContainsKey((int)_slotData.SlotCategory))
            {
                dicSlotTransform[(int)_slotData.SlotCategory] = new Queue<Transform>();
            }
            for (int i = 0; i < _slotData.SlotCount; i++)
            {
                GameObject slot = Instantiate(_slotData.SlotPrefab, _slotParent);
                dicSlotTransform[(int)_slotData.SlotCategory].Enqueue(slot.transform);
            }
        }
    }

    [SerializeField]
    private InventorySlot[] _inventorySlots;
    [SerializeField]
    private Renderer _itemUIRendererPrefab;
    [SerializeField] 
    private string _itemTextureProperty = "_Item_Tex";
    
    private int _itemTexturePropertyID;
    private readonly Dictionary<int, Queue<Transform>> _dicSlotTransform = new();
    private readonly Dictionary<ulong, ItemBase> _dicEquippedItem = new();
    private void Awake()
    {
        _itemTexturePropertyID = Shader.PropertyToID(_itemTextureProperty);
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            _inventorySlots[i].Init(_dicSlotTransform);
        }
    }

    private bool TryGetSlotTransform(out Transform parent, ItemCategory category)
    {
        parent = null;
        if (_dicSlotTransform == null)
            return false;

        bool bGet = _dicSlotTransform.TryGetValue((int)category, out Queue<Transform> slotQueue);
        if (slotQueue != null && slotQueue.Count > 0)
        {
            parent = slotQueue.Dequeue();
        }
        return bGet;
    }
    
    public void PushItem(ItemBase item)
    {
        if (!TryGetSlotTransform(out Transform parent, item.Category))
            return;
        Renderer newItemUIRenderer = Instantiate(_itemUIRendererPrefab, parent);
        ItemData itemData = item.GetItemData();
        MPBSystem.ChangeMaterialProperty(newItemUIRenderer, _itemTexturePropertyID, itemData.ItemTex);
        
        _dicEquippedItem.Add(itemData.ItemId, item);
    }
}
