using UnityEngine;

[CreateAssetMenu(fileName = "new FireCannonData", menuName = "WeaponData/FireCannonData")]
public class FireCannonData : WeaponData
{
    [SerializeField]
    private BulletData _prefabBulletData;
    [SerializeField]
    private PoolSize _bulletPoolSize;
    [SerializeField]
    private float _bulletLifeTime;

    public BulletData BulletData => _prefabBulletData;
    public PoolSize BulletPoolSize => _bulletPoolSize;
    public float BulletLifeTime => _bulletLifeTime;
}
