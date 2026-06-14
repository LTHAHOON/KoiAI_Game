using UnityEngine;

[CreateAssetMenu(fileName = "new CannonBallData", menuName = "ProjectileData/CannonBallData")]
public class CannonBallData : ProjectileData
{
    [SerializeField]
    private CannonBallController cannonBallControllerData;
    [SerializeField]
    private PoolSize _cannonBallPoolSize;
    [SerializeField]
    private float _cannonBallLifeTime;
    [SerializeField] 
    private int _maxOverlapCount;
    [SerializeField] 
    private float _radiusExplosion;

    public CannonBallController ControllerData => cannonBallControllerData;
    public PoolSize CannonBallPoolSize => _cannonBallPoolSize;
    public float CannonBallLifeTime => _cannonBallLifeTime;
    public int MaxOverlapCount => _maxOverlapCount;
    public float RadiusExplosion => _radiusExplosion;
}
