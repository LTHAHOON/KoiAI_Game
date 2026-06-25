using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ItemCategory
{
    Weapon,
    Resource
}

public class InventorySystem : MonoBehaviour
{
    [Serializable]
    public class InventorySlot
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

        #region 인벤토리 슬롯 관리 함수
        public void Init(MonoBehaviour caller, StringBuilder sb, GameObject selectedOutlinePrefab)
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
                if (i == 0)
                {
                    //LayerOut 설정 프레임 문제로 코루틴으로 호출
                    caller.StartCoroutine(IESelectSlot(slot));
                }
                else
                {
                    //DeSelectSlot(slot);
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

        public void DeSelectSlot(Slot slot)
        {
            slot.DeSelect();
        }

        public Slot GetEmptySlotFirst()
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

        public Slot GetSlot(ItemBase item)
        {
            #region 해당 아이템의 슬롯 가져오기
            for (int i = 0; i < _slotList.Count; i++)
            {
                ItemBase itemToCompare = _slotList[i].GetItem();
                if(!itemToCompare)
                {
                    continue;
                }
                if (itemToCompare == item)
                {
                    return _slotList[i];
                }
            }
            return null;
            #endregion
        }
        #endregion

        #region 반환형 프로퍼티
        public Slot SelectedSlot => _selectedSlot;
        public List<Slot> SlotList => _slotList;
        public Transform SlotParent => _slotParent;
        public SlotData SlotData => _slotData;
        #endregion
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
    private readonly Dictionary<int, List<ItemBase>> _dicPushedItem = new();
    private readonly HashSet<Slot> _pushedSlotSet = new();
    private readonly StringBuilder _stringBuilder = new();
    private void Awake()
    {
        _itemTexturePropertyID = Shader.PropertyToID(_itemTextureProperty);
        for (int i = 0; i < _inventorySlots.Length; i++)
        {
            InventorySlot inventorySlot = _inventorySlots[i];
            ItemSlotType itemSlotType = _inventorySlots[i].SlotData.ItemSlotType;
            _dicPushedItem.Add((int)itemSlotType, new List<ItemBase>());
            _dicInventorySlot.TryAdd((int)itemSlotType, inventorySlot);
            _dicInventorySlot[(int)itemSlotType].Init(this, _stringBuilder, _selectedOutlinePrefab);
        }
    }

    public bool TryGetInventorySlot(ItemSlotType itemSlotType, out InventorySlot inventorySlot)
    {
        return _dicInventorySlot.TryGetValue((int)itemSlotType, out inventorySlot);
    }

    public Slot GetSlotWithItem(ItemBase item)
    {
        ItemSlotType slotType = item.GetCurrentSlotType();
        if (!TryGetInventorySlot(slotType, out var inventorySlot))
        {
            return null;
        }
        Slot slot = inventorySlot.GetSlot(item);
        return slot;
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

    /// <summary>
    /// 해당 슬롯에 있는 아이템 삭제하기
    /// </summary>
    public void RemoveItem(Slot targetSlot, ItemSlotType itemSlotType)
    {
        //Slot이 비어있을 경우
        if (!_pushedSlotSet.Contains(targetSlot))
            return;
        if (!_dicInventorySlot.ContainsKey((int)itemSlotType))
            return;

        ItemBase item = targetSlot.GetItem();
        _pushedSlotSet.Remove(targetSlot);
        _dicPushedItem[(int)itemSlotType].Remove(item);
        targetSlot.ClearSlot(true);
    }

    /// <summary>
    /// 아이템 생성하고 넣기(새로 아이템 생성)
    /// </summary>
    public void CreateAndPushItem(PlayerController itemOwner, Transform itemParent, ItemSlotType itemSlotType, ItemData itemData)
    {
        if (!_dicPushedItem.ContainsKey((int)itemSlotType))
            return;
        if (!TryGetEmptySlotFirst(out Slot newSlot, itemSlotType))
            return;

        Renderer itemUI = CreateItemUI(newSlot, itemData.ItemTex);
        ItemBase newItem = CreateItem(itemOwner, itemParent, itemUI, itemSlotType, itemData);
        if(newItem == null)
        {
            return;
        }

        newSlot.PushItem(newItem);
        _pushedSlotSet.Add(newSlot);
        _dicPushedItem[(int)itemSlotType].Add(newItem);
        newItem.SetItemCountInSlot();
    }

    /// <summary>
    /// 아이템 UI 생성
    /// </summary>
    private Renderer CreateItemUI(Slot targetSlot, Texture2D texture)
    {
        Renderer newItemUIRenderer = Instantiate(_itemUIRendererPrefab, targetSlot.transform);
        //아이템 텍스처 넣기
        MPBSystem.ChangeMaterialProperty(newItemUIRenderer, _itemTexturePropertyID, texture);
        return newItemUIRenderer;
    }

    /// <summary>
    /// 아이템 생성(슬롯에 아이템이 없는 상태)
    /// </summary>
    private ItemBase CreateItem(PlayerController itemOwner, Transform itemParent, Renderer itemUI, ItemSlotType itemSlotType, ItemData itemData)
    {
        ItemBase newItem;
        newItem = Instantiate(itemData.ItemPrefab, itemParent);

        //형태가 보여져야 할 아이템이 아닐 경우
        if (!itemData.IsCreatableObj)
        {
            newItem.gameObject.SetActive(false);
        }
        newItem.Init(itemOwner, itemUI, itemSlotType);
        return newItem;
    }

    /// <summary>
    /// 아이템 넣기(슬롯에 아이템이 있는 상태)
    /// </summary>
    public void PushItem(Slot curSlot, ItemSlotType slotTypeToPush, Slot slotToPush = null)
    {
        if (!_dicPushedItem.ContainsKey((int)slotTypeToPush))
            return;
        if (slotToPush == null)
        {
            if (!TryGetEmptySlotFirst(out Slot slot, slotTypeToPush))
                return;
            slotToPush = slot;
        }
        else
        {
            //넣을 위치의 Slot에 아이템이 있을 경우
            if (_pushedSlotSet.Contains(slotToPush))
                return;
        }
        
        ItemBase item =  curSlot.GetItem();
        ItemSlotType curSlotType = item.GetCurrentSlotType();

        slotToPush.PushItem(item);
        curSlot.ClearSlot(false);

        _pushedSlotSet.Remove(curSlot);
        _pushedSlotSet.Add(slotToPush);

        _dicPushedItem[(int)curSlotType].Remove(item);
        _dicPushedItem[(int)slotTypeToPush].Add(item);

        item.SetCurrentSlotType(slotTypeToPush);
        item.SetItemCountInSlot();
    }
    
}
