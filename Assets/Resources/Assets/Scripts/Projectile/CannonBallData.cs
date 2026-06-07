using UnityEngine;

[CreateAssetMenu(fileName = "new CannonBallData", menuName = "ProjectileData/CannonBallData")]
public class CannonBallData : ProjectileData
{
    [SerializeField]
    private PoolSize _bulletPoolSize;
    [SerializeField]
    private float _bulletLifeTime;

    public PoolSize BulletPoolSize => _bulletPoolSize;
    public float BulletLifeTime => _bulletLifeTime;
}
