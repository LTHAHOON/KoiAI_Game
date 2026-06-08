using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static InventorySystem;

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
        [SerializeField]
        private bool _isUseSelectedOutline;

        private GameObject _selectedOutline;
        private RectTransform _selectedOutlineRect;
        private List<Slot> _slotList;
        private Slot _selectedSlot;
        public void Init(InventorySystem caller,StringBuilder sb, GameObject selectedOutlinePrefab)
        {
            #region  슬롯 데이터 및 슬롯 선택 아웃라인 세팅
            if (!_slotData || !_slotParent)
                return;
            if (_isUseSelectedOutline)
            {
                _selectedOutline = Instantiate(selectedOutlinePrefab, _slotParent);
                _selectedOutlineRect = _selectedOutline.GetComponent<RectTransform>();
            }
            ItemSlotType itemSlotType = _slotData.ItemSlotType;
            _slotList= new ();

            for (int i = 0; i < _slotData.SlotCount; i++)
            {
                Slot slot = Instantiate(_slotData.SlotPrefab, _slotParent);
                slot.Init(i, itemSlotType, sb);
                _slotList.Add(slot);
                sb.Clear();
                if(i == 0)
                {
                    //LayerOut 설정 프레임 문제로 코루틴으로 호출
                    slot.StartCoroutine(IESelectSlot(slot));
                }    
            }

            #endregion
        }

        private IEnumerator IESelectSlot(Slot slot)
        {
            yield return new WaitForEndOfFrame();
            SelectSlot(slot);
        }

        public void SelectSlot(Slot slot)
        {
            #region SelectedOutline 위치를 Slot 위치로 옮기기
            if (!_isUseSelectedOutline)
                return;
            _selectedSlot = slot;
            slot.Select(_selectedOutlineRect);
            #endregion
        }

        public readonly void DeSelectSlot(Slot slot)
        {
            slot.DeSelect();
        }

        public readonly Slot GetEmptySlotFirst()
        {
            #region 비어있는 슬롯들에서 제일 첫번째 슬롯 가져오기
            if (_slotList == null)
                return null;
            for (int i = 0; i < _slotList.Count; i++)
            {
                if (_slotList[i].IsEmpty())
                {
                    return _slotList[i];
                }
            }
            return null;
            #endregion
        }
        public readonly Slot SelectedSlot => _selectedSlot;
        public readonly List<Slot> SlotList => _slotList;
        public readonly Transform SlotParent => _slotParent;
        public readonly SlotData SlotData => _slotData;
    }

    [SerializeField]
    private InventorySlot[] _inventorySlots;
    [SerializeField]
    private Renderer _itemUIRendererPrefab;
    [SerializeField] 
    private string _itemTextureProperty = "_Item_Tex";
    [SerializeField]
    private GameObject _selectedOutlinePrefab;

    private int _itemTexturePropertyID;
    private readonly Dictionary<int, InventorySlot> _dicInventorySlot = new();
    private readonly Dictionary<ulong, ItemBase> _dicPushedItem = new();
    private readonly HashSet<Slot> _pushedSlotSet = new();
    private readonly StringBuilder _stringBuilder = new();
    private void Awake()
    {
        _itemTexturePropertyID = Shader.PropertyToID(_itemTextureProperty);
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            InventorySlot inventorySlot = _inventorySlots[i];
            inventorySlot.Init(this,_stringBuilder, _selectedOutlinePrefab);
            _dicInventorySlot.Add((int)inventorySlot.SlotData.ItemSlotType, inventorySlot);
        }
    }

    public bool TryGetInventorySlot(ItemSlotType itemSlotType, out InventorySlot inventorySlot)
    {
        return _dicInventorySlot.TryGetValue((int)itemSlotType, out inventorySlot);
    }

    private bool TryGetEmptySlotFirst(out Slot slot, ItemSlotType itemSlotType)
    {
        slot = null;
        if (_dicInventorySlot == null)
            return false;

        bool bGet = TryGetInventorySlot(itemSlotType, out InventorySlot inventorySlot);
        if (bGet)
        {
            Slot emptySlot = inventorySlot.GetEmptySlotFirst();
            if(emptySlot)
            {
                slot = emptySlot;
            }
        }
        return bGet;
    }

    public void SelectItemSlot(ItemSlotType itemSlotType, int index)
    {
        bool bGet = TryGetInventorySlot(itemSlotType, out InventorySlot inventorySlot);
        if (!bGet)
        {
            return;
        }
        Slot slot = FindSlotWithIndex(inventorySlot, index);
        if (slot)
        {
            inventorySlot.SelectSlot(slot);
        }
    }
    public void DeSelectItemSlot(ItemSlotType itemSlotType, int index)
    {
        bool bGet = TryGetInventorySlot(itemSlotType, out InventorySlot inventorySlot);
        if (!bGet)
        {
            return;
        }
        Slot slot = FindSlotWithIndex(inventorySlot, index);
        if(slot)
        {
            inventorySlot.DeSelectSlot(slot);
        }
    }

    /// <summary>
    /// 슬롯 인덱스로 해당 슬롯 찾기(없으면 NULL)
    /// </summary>
    public Slot FindSlotWithIndex(InventorySlot inventorySlot, int indexToFind)
    {
        var slotList = inventorySlot.SlotList;
        if (slotList != null)
        {
            for (int i = 0; i < slotList.Count; i++)
            {
                Slot slot = slotList[i];
                if (slot.CompareIndex(indexToFind))
                {
                    return slot;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 선택된 인덱스 찾기(없으면 -1)
    /// </summary>
    public int FindSelectedIndex(ItemSlotType itemSlotType)
    {
        bool bGet = TryGetInventorySlot(itemSlotType, out InventorySlot inventorySlot);
        if (!bGet)
        {
            return -1;
        }
        Slot slot = inventorySlot.SelectedSlot;
        if(slot && slot.IsSelected())
        {
            return slot.SlotIndex;
        }
        return -1;
    }

    public void RemoveItem(Slot targetSlot, ItemSlotType itemSlotType)
    {
        //Slot이 비어있을 경우
        if (!_pushedSlotSet.Contains(targetSlot))
            return;
        if (!_dicInventorySlot.ContainsKey((int)itemSlotType))
            return;
        _pushedSlotSet.Remove(targetSlot);
        targetSlot.ClearSlot();
    }
    
    //생성하고 아이템 넣기
    public Slot CreateAndPushItem(PlayerController itemOwner, Transform itemParent, ItemData itemData)
    {
        if (!TryGetEmptySlotFirst(out Slot slot, itemData.Type))
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
