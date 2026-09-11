using System;
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
                Instantiate(_hitEffectPrefab, hitPoint, Quaternion.identity);
            }
        
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _overlapRadius, _targetColliders, _damageTargetLayerMask);
            hitCount = Math.Min(hitCount, _maxOverlapCount);
            OnHit?.Invoke(_targetColliders, hitCount);
        }
    
    }
}
