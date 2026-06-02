using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct WeaponBaseData
{
    [SerializeField]
    private Image _weaponImage;
    [SerializeField]
    private string _weaponName;
    [SerializeField]
    private ulong _weaponId;
    [SerializeField]
    private float _weaponDamage;
}
public class WeaponData : ScriptableObject
{
    [SerializeField]
    private WeaponBaseData _weaponBaseData;
    public WeaponBaseData WeaponBaseData => _weaponBaseData;
}
