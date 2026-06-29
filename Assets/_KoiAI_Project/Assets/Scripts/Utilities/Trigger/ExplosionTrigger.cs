using System;
using UnityEngine;

namespace KoiAI.Utilities
{
    public class ExplosionTrigger : MonoBehaviour
    {
        [Header("Trigger Target Layer Mask")]
        [SerializeField]
        private LayerMask _triggerTargetLayerMask = UnityEngine.Physics.AllLayers;
        [Header("Damage Target Layer Mask")]
        [SerializeField]
        private LayerMask _damageTargetLayerMask;
        [SerializeField]
        private GameObject _explosionPrefab;
    
        private bool _isTriggerEnter = false;
        private int _maxOverlapCount = 0;
        private float _overlapRadius = 0f;
        private Collider[] _targetColliders;
        private Action<Collider[], int> OnExplosion;
    
        public void Init(int maxOverlapCount, float overlapRadius,  Action<Collider[], int>  explosionEvent, int targetLayerMask = -1)
        {
            if (targetLayerMask != -1)
            {
                _damageTargetLayerMask = targetLayerMask;
            }
            _overlapRadius = overlapRadius;
            _maxOverlapCount = maxOverlapCount;
            _targetColliders = new Collider[maxOverlapCount];
            OnExplosion += explosionEvent;
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
            if (_targetColliders == null)
            {
                _targetColliders = new Collider[_maxOverlapCount];
            }
        
            bool isTriggerEnter = UnityEngine.Physics.CheckSphere(transform.position, _overlapRadius, _triggerTargetLayerMask);
            _isTriggerEnter = isTriggerEnter;
            if (!isTriggerEnter)
            {
                return;
            }
        
            Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
            if(_explosionPrefab)
            {
                Instantiate(_explosionPrefab, hitPoint, Quaternion.identity);
            }
        
            int damgeTargetCount = UnityEngine.Physics.OverlapSphereNonAlloc(transform.position, _overlapRadius, _targetColliders, _damageTargetLayerMask);
            OnExplosion?.Invoke(_targetColliders, damgeTargetCount);
        }
    
    }
}
