using System;
using UnityEngine;
using UnityEngine.UI;

public class WeaponData : ItemData
{
    [SerializeField]
    private float _weaponDamage;
}

public class ProjectileData : ItemData
{
    [SerializeField]
    private int _projectileMaxCount;
    [SerializeField]
    private float _projectileDamage;

    public float Damage => _projectileDamage;
    public int Count => _projectileMaxCount;
}

public abstract class ItemData : ScriptableObject
{
    [SerializeField]
    private ItemBase _itemPrefab;
    [SerializeField]
    private Texture2D _itemTex;
    [SerializeField]
    private string _itemName;
    [SerializeField]
    private ulong _itemId;
    //장착 가능한지 구분하기 위한 타입
    [SerializeField]
    private ItemSlotType _type;

    public ItemBase ItemPrefab => _itemPrefab;
    public ItemSlotType Type => _type;
    public Texture2D ItemTex => _itemTex;
    public string ItemName => _itemName;
    public ulong ItemId => _itemId;
}
