using System;
using UnityEngine;
using UnityEngine.UI;

public class WeaponData : ItemData
{
    [SerializeField]
    private float _weaponDamage;
}

public abstract class ItemData : ScriptableObject
{
    [SerializeField]
    private Texture2D _itemTex;
    [SerializeField]
    private string _itemName;
    [SerializeField]
    private ulong _itemId;
    public Texture2D ItemTex => _itemTex;
    public string ItemName => _itemName;
    public ulong ItemId => _itemId;
}
