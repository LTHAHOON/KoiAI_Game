using UnityEngine;

[CreateAssetMenu(fileName = "new CannonBallData", menuName = "ProjectileData/CannonBallData")]
public class CannonBallData : ProjectileData
{
    [SerializeField]
    private CannonBallSkin _cannonBallSkinData;
    [SerializeField]
    private PoolSize _cannonBallPoolSize;
    [SerializeField]
    private float _cannonBallLifeTime;

    public CannonBallSkin SkinData => _cannonBallSkinData;
    public PoolSize CannonBallPoolSize => _cannonBallPoolSize;
    public float CannonBallLifeTime => _cannonBallLifeTime;
}
