using KoiAI.Item;
using KoiAI.Skin;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI
{
    public class BulletController : MonoBehaviour
    {
        private BulletSkin _bulletSkin;
        [SerializeField]
        private GameObject _hitEffectPrefab;
        private BulletData _bulletData;
        public bool Init(BulletData bulletData)
        {
            _bulletData = bulletData;
            if (!_bulletSkin)
            {
                _bulletSkin = GetComponentInChildren<BulletSkin>();
            }

            if (!_bulletSkin)
            {
                Debug.LogError($"{nameof(BulletController)} requires a {nameof(BulletSkin)}.", this);
                return false;
            }
            return true;
        }

        public void SetSkin(BulletSkin bulletSkin)
        {
            _bulletSkin = bulletSkin;
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
