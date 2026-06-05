using System;
using System.Text;
using TMPro;
using UnityEngine;

[Serializable]
public enum SlotType
{
    notEquipped,
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

    public void SetItemCountText(string number)
    {
        _itemCountText.text = number;
    }

    public void SetItemCountText(StringBuilder sb)
    {
        _itemCountText.text = sb.ToString();
    }
}
