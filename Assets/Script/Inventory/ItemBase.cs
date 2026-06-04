using Mono.Cecil;
using UnityEngine;

public abstract class ItemBase: MonoBehaviour
{
    public abstract ItemCategory Category { get; }
    public abstract ItemData GetItemData();
    public T GetController<T>()
    {
        if (this is not T controller)
            return default;
        return controller;
    }
}
public abstract class WeaponItemBase : ItemBase
{
    public override ItemCategory Category => ItemCategory.Weapon;
}
public abstract class ResourceItemBase : ItemBase
{
    public override ItemCategory Category => ItemCategory.Resource;
}
