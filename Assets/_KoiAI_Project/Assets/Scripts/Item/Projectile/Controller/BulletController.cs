using KoiAI.Item;
using KoiAI.Pool;
using KoiAI.Skin;
using KoiAI.Utilities;
using UnityEngine;

namespace KoiAI
{
    public class BulletController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _hitEffectPrefab;
        [SerializeField]
        private PoolSize _hitEffectPoolSize = new(5, 20);
        [SerializeField, Min(0f)]
        private float _hitEffectLifetime = 2f;
        
        private BulletSkin _bulletSkin;
        private BulletData _bulletData;
        private Pool<GameObject> _hitEffectPool;
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

            EnsureHitEffectPool();
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
                EnsureHitEffectPool();
                GameObject hitEffect = _hitEffectPool?.Pop();
                if (hitEffect)
                {
                    hitEffect.transform.SetPositionAndRotation(hit.point, Quaternion.identity);
                    PoolManager.Instance.ReturnDelay(_hitEffectPool, hitEffect, Mathf.Max(0f, _hitEffectLifetime));
                }
                else
                {
                    Instantiate(_hitEffectPrefab, hit.point, Quaternion.identity);
                }
            }
            if (hit.collider.TryGetComponent(out Health.Health health))
            {
                
                health.ChangeHealth(-_bulletData.Damage);
            }
        }

        public BulletSkin GetBulletSkin() => _bulletSkin;

        private void EnsureHitEffectPool()
        {
            if (_hitEffectPool != null || !_hitEffectPrefab || PoolManager.Instance == null)
            {
                return;
            }

            ulong poolID = unchecked((ulong)(uint)_hitEffectPrefab.GetInstanceID());
            PoolManager.Instance.AddPool(poolID, _hitEffectPrefab, _hitEffectPoolSize, PoolName.HitEffect);
            PoolManager.Instance.TryGetPool(poolID, out _hitEffectPool);
        }
    }
}
