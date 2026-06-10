using UnityEngine;

public class CannonSkin : WeaponSkin
{
    [SerializeField]
    private Transform _firePoint;

    public Transform FirePoint => _firePoint;
}
