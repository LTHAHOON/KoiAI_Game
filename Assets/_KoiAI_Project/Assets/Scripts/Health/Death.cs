using System;
using Cysharp.Threading.Tasks;
using KoiAI.Core;
using UnityEngine;

namespace KoiAI.Health
{
    [RequireComponent(typeof(Health), typeof(Collider), typeof(EntityIdentity))]
    public abstract class Death : MonoBehaviour
    {
        private Health _health;
        [SerializeField]
        private LayerMask _excludeLayersWhenDie;
        [SerializeField]
        private float _destroyDuration = 1f;

        private EntityIdentity _myIdentity;
        private Rigidbody _rigidbody;
        private Collider _collider;
        
        private void Awake()
        {
            _health = GetComponent<Health>();
            _myIdentity = GetComponent<EntityIdentity>();
            _collider = GetComponent<Collider>();
            TryGetComponent<Rigidbody>(out _rigidbody);
            _health.OnDeath += OnDeath;
            Initialize();
        }

        public async void OnDeath(EntityIdentity instigatorIdentity)
        {
            GameplayEvents.OnKilled?.Invoke(new ObjectiveProgressEventData(instigatorIdentity, _myIdentity));
            if(_rigidbody)
            {
                _rigidbody.excludeLayers = _excludeLayersWhenDie;
            }
            _collider.excludeLayers = _excludeLayersWhenDie;
            OnDeath(_health);
            await UniTask.Delay(TimeSpan.FromSeconds(_destroyDuration));
            Destroy(_health.gameObject);

        }

        public abstract void Initialize();
        public abstract void OnDeath(Health health);
    }
}
