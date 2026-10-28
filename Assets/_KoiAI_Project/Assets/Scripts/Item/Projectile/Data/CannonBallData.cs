using UnityEngine;

namespace KoiAI.Item
{
    using KoiAI.Pool;
    using KoiAI.Skin;

    [CreateAssetMenu(fileName = "new CannonBallData", menuName = "ProjectileData/CannonBallData")]
    public class CannonBallData : ProjectileData
    {
        [SerializeField]
        private CannonBallSkin _cannonBallSkin;
        [SerializeField]
        private PoolSize _cannonBallPoolSize;
        [SerializeField]
        private float _cannonBallLifeTime;
        [SerializeField] 
        private int _maxOverlapCount;
        [SerializeField] 
        private float _radiusExplosion;

        public CannonBallSkin CannonBallSkin => _cannonBallSkin;
        public PoolSize CannonBallPoolSize => _cannonBallPoolSize;
        public float CannonBallLifeTime => _cannonBallLifeTime;
        public int MaxOverlapCount => _maxOverlapCount;
        public float RadiusExplosion => _radiusExplosion;
    }
}
