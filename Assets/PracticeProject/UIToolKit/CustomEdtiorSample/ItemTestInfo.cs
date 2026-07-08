using System;
using UnityEngine;

public enum ItemType
{
    Weapon,
    Armor,
    Portion,
    Food,
}

#pragma warning disable CS0108
[CreateAssetMenu(fileName = "new ItemTestInfo", menuName = "ItemTestInfo")]
public class ItemTestInfo : ScriptableObject
{
    public string id = Guid.NewGuid().ToString().ToUpper();
    public string name;
    public Sprite icon;
    public ItemType itemType;
    public string info;
    public int gold;
}
