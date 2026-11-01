using System;
using KoiAI.Pool;
using UnityEngine;

namespace KoiAI.Utilities
{
    public class HitEffectTrigger : MonoBehaviour
    {
        [Header("Trigger Target Layer Mask")]
        [SerializeField]
        private LayerMask _triggerTargetLayerMask = Physics.AllLayers;
        [Header("Damage Target Layer Mask")]
        [SerializeField]
        private LayerMask _damageTargetLayerMask;
        [SerializeField]
        private GameObject _hitEffectPrefab;
        [SerializeField]
        private PoolSize _hitEffectPoolSize = new(5, 20);
        [SerializeField, Min(0f)]
        private float _hitEffectLifetime = 2f;
        private Pool<GameObject> _hitEffectPool;
    
        private bool _isTriggerEnter = false;
        private int _maxOverlapCount = 0;
        private float _overlapRadius = 0f;
        private Collider[] _targetColliders;
        private Action<Collider[], int> OnHit;
    
        public void Init(int maxOverlapCount, float overlapRadius,  Action<Collider[], int>  hitCount, int targetLayerMask = -1)
        {
            if (targetLayerMask != -1)
            {
                _damageTargetLayerMask = targetLayerMask;
            }
            _overlapRadius = overlapRadius;
            _maxOverlapCount = maxOverlapCount;
            _targetColliders = new Collider[maxOverlapCount];
            OnHit += hitCount;
            EnsureHitEffectPool();
        }
    
        private void OnDisable()
        {
            _isTriggerEnter = false;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_isTriggerEnter)
            {
                return;
            }
            bool isTriggerEnter = Physics.CheckSphere(transform.position, _overlapRadius, _triggerTargetLayerMask);
            _isTriggerEnter = isTriggerEnter;
            if (!isTriggerEnter)
            {
                return;
            }
        
            Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
            if(_hitEffectPrefab)
            {
                EnsureHitEffectPool();
                GameObject hitEffect = _hitEffectPool?.Pop();
                if (hitEffect)
                {
                    hitEffect.transform.SetPositionAndRotation(hitPoint, Quaternion.identity);
                    PoolManager.Instance.ReturnDelay(_hitEffectPool, hitEffect, Mathf.Max(0f, _hitEffectLifetime));
                }
                else
                {
                    Instantiate(_hitEffectPrefab, hitPoint, Quaternion.identity);
                }
            }
        
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _overlapRadius, _targetColliders, _damageTargetLayerMask);
            hitCount = Math.Min(hitCount, _maxOverlapCount);
            OnHit?.Invoke(_targetColliders, hitCount);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, _overlapRadius);
        }

        private void EnsureHitEffectPool()
        {
            if (_hitEffectPool != null || !_hitEffectPrefab || PoolManager.Instance == null)
            {
                return;
            }

            ulong poolID = _hitEffectPrefab.GetEntityULongID();
            PoolManager.Instance.AddPool(poolID, _hitEffectPrefab, _hitEffectPoolSize, PoolName.HitEffect);
            PoolManager.Instance.TryGetPool(poolID, out _hitEffectPool);
        }
    }
}
