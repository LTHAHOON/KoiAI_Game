using KoiAI.Item;
using KoiAI.Skin;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI
{
    public class BulletController : MonoBehaviour
    {
        [SerializeField]
        private BulletSkin _bulletSkin;
        [SerializeField]
        private GameObject _hitEffectPrefab;

        private BulletData _bulletData;
        public void Init(BulletData bulletData, LayerMask targetLayerMask)
        {
            _bulletData = bulletData;
        }

        public void OnHit(RaycastHit[] hits, int hitCount)
        {
            if (!_bulletData)
            {
                return;
            }
            if (hitCount <= 0)
            {
                return;
            }

            for (int i = 0; i < hitCount; i++)
            {
                if (!hits[i].collider)
                {
                    continue;
                }
                if (_hitEffectPrefab)
                {
                    Instantiate(_hitEffectPrefab, hits[i].point, Quaternion.identity);
                }
                if (hits[i].collider.TryGetComponent(out Health.Health health))
                {
                    health.ChangeHealth(-_bulletData.Damage);
                }
            }
        }

        public BulletSkin GetBulletSkin() => _bulletSkin;
    }
}
