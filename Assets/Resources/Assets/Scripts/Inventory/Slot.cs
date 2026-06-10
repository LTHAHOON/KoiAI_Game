using System;
using System.Text;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

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
    private RectTransform _itemDataParent;

    private ItemBase _item;
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
        {
            return;
        }
        selectedOutlineRect.localPosition = _rectTransform.localPosition;
        _isSelected = true;
        SetItemInSlotActive(true);
    }

    public void DeSelect()
    {
        _isSelected = false;
        SetItemInSlotActive(false);
    }

    public void PushItem(ItemBase item)
    {
        Renderer itemUI = item.GetItemUI();
        itemUI.transform.SetParent(_itemDataParent);
        itemUI.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _item = item;
        SetItemInSlotActive(_isSelected);
    }
    
    private void SetItemInSlotActive(bool active)
    {
        if (_item)
        {
            _item.SetItemActive(active);
        }
    }

    public void ClearSlot(bool bDestroy)
    {
        if(bDestroy)
        {
            if (_item != null && _item.gameObject.scene.IsValid() && _item.gameObject.scene.isLoaded)
            {
                Destroy(_item.GetItemUI());
                Destroy(_item.gameObject);
            }
        }
        else
        {
            _item = null;
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
