using System;
using System.Text;
using TMPro;
using UnityEngine;

[Serializable]
public enum ItemSlotType
{
    NotEquipped,
    Equipped,
}
[RequireComponent(typeof(RectTransform))]
public class Slot : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _itemCountText;
    [SerializeField]
    private Transform _itemDataParent;

    private ItemBase _item;
    private Renderer _itemUI;
    private int _slotIndex;
    private RectTransform _rectTransform;
    private bool _isSelected = false;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Init(int slotIndex, ItemSlotType itemSlotType, StringBuilder sb)
    {
        _slotIndex = slotIndex;
        if (itemSlotType == ItemSlotType.Equipped)
        {
            sb.Append(slotIndex + 1);
            SetItemCountText(sb);
        }
    }
    public void Select(RectTransform selectedOutlineRect)
    {
        if (!_rectTransform || !selectedOutlineRect)
            return;
        selectedOutlineRect.anchoredPosition = _rectTransform.anchoredPosition;
        _isSelected = true;
    }
    public void DeSelect()
    {
        _isSelected = false;
    }
    public void PushItem(ItemBase item, Renderer itemUI)
    {
        _item = item;
        _itemUI = itemUI;
        _itemUI.transform.SetParent(_itemDataParent);
    }
    
    public void ClearSlot()
    {
        Destroy(_itemUI.gameObject);
        if (_item != null && _item.gameObject.scene.IsValid() && _item.gameObject.scene.isLoaded)
        {
            Destroy(_item.gameObject);
        }
    }
    

    public void SetItemCountText(StringBuilder sb)
    {
        _itemCountText.SetText(sb);
    }

    public ItemBase GetItem()
    {
        if(IsEmpty())
            return null;
        return _item;
    }
    
    ///<summary>
    ///슬롯 인덱스가 같은 경우 True 아닐 경우 False
    ///</summary>
    public bool CompareIndex(int index) => _slotIndex == index;

    ///<summary>
    ///슬롯에 아이템이 없을 경우 True 아닐 경우 False
    ///</summary>
    public bool IsEmpty() => _item == null;
    public bool IsSelected() => _isSelected;
    public int SlotIndex => _slotIndex;
}
