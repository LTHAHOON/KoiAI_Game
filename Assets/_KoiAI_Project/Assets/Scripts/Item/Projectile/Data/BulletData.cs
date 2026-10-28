using KoiAI.Item;
using KoiAI.Pool;
using UnityEngine;

namespace KoiAI
{
    using KoiAI.Skin;
    [CreateAssetMenu(fileName = "new BulletData", menuName = "ProjectileData/BulletData")]
    public class BulletData : ProjectileData
    {
        [SerializeField]
        private BulletSkin _bulletSkin;
        [SerializeField]
        private PoolSize _bulletPoolSize;
        [SerializeField]
        private int _maxOverlapCount;

        public BulletSkin BulletSkin => _bulletSkin;
        public PoolSize BulletPoolSize => _bulletPoolSize;
        public int MaxOverlapCount => _maxOverlapCount;
    }
}
