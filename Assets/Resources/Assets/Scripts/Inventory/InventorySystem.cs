using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditorInternal.Profiling.Memory.Experimental;
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

        public void Init(Dictionary<int, Queue<Slot>> dicSlot, StringBuilder sb)
        {
            if (!_slotData || !_slotParent)
                return;

            ItemSlotType itemSlotType = _slotData.ItemSlotType;
            if (!dicSlot.ContainsKey((int)itemSlotType))
            {
                dicSlot[(int)itemSlotType] = new Queue<Slot>();
            }

            for (int i = 0; i < _slotData.SlotCount; i++)
            {
                Slot slot = Instantiate(_slotData.SlotPrefab, _slotParent);
                if(itemSlotType == ItemSlotType.Equipped)
                {
                    sb.Append(i + 1);
                    slot.SetItemCountText(sb);
                }
                dicSlot[(int)itemSlotType].Enqueue(slot);
                sb.Clear();
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
    private readonly Dictionary<int, Queue<Slot>> _dicSlot = new();
    private readonly Dictionary<ulong, ItemBase> _dicPushedItem = new();
    private readonly HashSet<Slot> _pushedSlotSet = new();
    private readonly StringBuilder _stringBuilder = new();
    private void Awake()
    {
        _itemTexturePropertyID = Shader.PropertyToID(_itemTextureProperty);
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            _inventorySlots[i].Init(_dicSlot, _stringBuilder);
        }
    }

    private bool TryGetSlot(out Slot slot, ItemSlotType itemSlotType)
    {
        slot = null;
        if (_dicSlot == null)
            return false;

        bool bGet = _dicSlot.TryGetValue((int)itemSlotType, out Queue<Slot> slotQueue);
        if (slotQueue != null && slotQueue.Count > 0)
        {
            slot = slotQueue.Dequeue();
        }
        return bGet;
    }

    public void RemoveItem(Slot targetSlot, ItemSlotType itemSlotType)
    {
        //Slot이 비어있을 경우
        if (!_pushedSlotSet.Contains(targetSlot))
            return;
        if (!_dicSlot.ContainsKey((int)itemSlotType))
            return;
        _pushedSlotSet.Remove(targetSlot);
        targetSlot.ClearSlot();
        _dicSlot[(int)itemSlotType].Enqueue(targetSlot);
    }
    
    //생성하고 아이템 넣기
    public Slot CreateAndPushItem(PlayerController itemOwner, Transform itemParent, ItemData itemData)
    {
        if (!TryGetSlot(out Slot slot, itemData.Type))
            return null;
        ItemBase newItem;
        if (itemData.Type == ItemSlotType.Equipped)
        {
            newItem   = Instantiate(itemData.ItemPrefab, itemParent);
        }
        else
        {
            newItem = itemData.ItemPrefab;
        }
        newItem.Init(itemOwner);    
        Renderer newItemUIRenderer = Instantiate(_itemUIRendererPrefab, slot.transform);
        //아이템 텍스처 넣기
        MPBSystem.ChangeMaterialProperty(newItemUIRenderer, _itemTexturePropertyID, itemData.ItemTex);
        
        PushItem(newItem, itemData,newItemUIRenderer, slot);
        return slot;

    }
    
    //아이템 넣기(이미 생성된 상태)
    public void PushItem(ItemBase newItem, ItemData itemData, Renderer itemUI, Slot targetSlot)
    {
        //Slot이 비어있지 않을 경우
        if (_pushedSlotSet.Contains(targetSlot))
            return;
        
        targetSlot.PushItem(newItem, itemUI);
        _pushedSlotSet.Add(targetSlot);
        _dicPushedItem.Add(itemData.ItemId, newItem);
    }
    
}
