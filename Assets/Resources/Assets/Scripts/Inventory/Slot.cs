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
public class Slot : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _itemCountText;
    [SerializeField]
    private Transform _itemDataParent;

    private ItemBase _item;
    private Renderer _itemUI;
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
    
    public void SetItemCountText(string number)
    {
        _itemCountText.text = number;
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
    public bool IsEmpty() => _item == null;
    
}
