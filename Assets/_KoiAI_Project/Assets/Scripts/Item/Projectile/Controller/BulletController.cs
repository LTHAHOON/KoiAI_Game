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
        public void Init(BulletData bulletData)
        {
            _bulletData = bulletData;
        }

        public void OnHit(RaycastHit hit)
        {
            if (!_bulletData || hit.collider == null)
            {
                return;
            }

            if (_hitEffectPrefab)
            {
                Instantiate(_hitEffectPrefab, hit.point, Quaternion.identity);
            }
            if (hit.collider.TryGetComponent(out Health.Health health))
            {
                
                health.ChangeHealth(-_bulletData.Damage);
            }
        }

        public BulletSkin GetBulletSkin() => _bulletSkin;
    }
}
