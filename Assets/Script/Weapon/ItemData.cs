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
    private Projectile _projectilePrefab;
    [SerializeField]
    private int _projectileMaxCount;
    [SerializeField]
    private int _projectileBaseCount;
    [SerializeField]
    private float _projectileDamage;

    public float Damage => _projectileDamage;
    public Projectile Prefab => _projectilePrefab;
    public int MaxCount => _projectileMaxCount;
    public int BaseCount => _projectileBaseCount;
}

public abstract class ItemData : ScriptableObject
{
    [SerializeField]
    private Texture2D _itemTex;
    [SerializeField]
    private string _itemName;
    [SerializeField]
    private ulong _itemId;
    //장착 가능한지 구분하기 위한 타입
    [SerializeField]
    private SlotType _type;

    public SlotType Type => _type;
    public Texture2D ItemTex => _itemTex;
    public string ItemName => _itemName;
    public ulong ItemId => _itemId;
}
