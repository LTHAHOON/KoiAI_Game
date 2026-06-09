using Mono.Cecil;
using UnityEngine;

public abstract class ItemBase: MonoBehaviour
{
    private PlayerController _itemOwner;
    private Renderer _itemUI;
    private ItemSlotType _currentSlotType;
    public abstract ItemCategory Category { get; }

    public virtual void Init(PlayerController itemOwner, Renderer itemUI, ItemSlotType curSlotType)
    {
        _itemUI = itemUI;
        SetCurrentSlotType(curSlotType);
        SetItemOwner(itemOwner);
    }
    protected void SetItemOwner(PlayerController itemOwner)
    {
        _itemOwner = itemOwner;
    }
    public void SetCurrentSlotType(ItemSlotType curSlotType)
    {
        _currentSlotType = curSlotType;
    }

    public bool TryGetItemChildClass<T>(out T item) where T : ItemBase
    {
        if(this is T itemClass)
        {
            item = itemClass;
            return true;
        }
        item = null;
        return false;
    }

    public Renderer GetItemUI() => _itemUI;
    public ItemSlotType GetCurrentSlotType() => _currentSlotType;
    public abstract void UseItem();
    public abstract ItemData GetItemData();
    public PlayerController ItemOwner => _itemOwner;
}
public abstract class WeaponBase : ItemBase
{
    public override ItemCategory Category => ItemCategory.Weapon;
}
public abstract class ResourceBase : ItemBase
{
    public override ItemCategory Category => ItemCategory.Resource;
}
