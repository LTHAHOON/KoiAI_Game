using Mono.Cecil;
using UnityEngine;

public abstract class ItemBase: MonoBehaviour
{
    private PlayerController _itemOwner;
    public abstract ItemCategory Category { get; }
    public abstract ItemData GetItemData();

    public virtual void Init(PlayerController itemOwner)
    {
        SetItemOwner(itemOwner);
    }
    protected void SetItemOwner(PlayerController itemOwner)
    {
        _itemOwner = itemOwner;
    }

    public PlayerController ItemOwner => _itemOwner;
}
public abstract class WeaponItemBase : ItemBase
{
    public abstract override void Init(PlayerController itemOwner);
    public override ItemCategory Category => ItemCategory.Weapon;
}
public abstract class ResourceItemBase : ItemBase
{
    public override ItemCategory Category => ItemCategory.Resource;
}
