using System;
using Cysharp.Threading.Tasks;
using KoiAI.Core;
using UnityEngine;

namespace KoiAI.Health
{
    [RequireComponent(typeof(Health), typeof(EntityIdentity))]
    public abstract class Death : MonoBehaviour
    {
        [SerializeField]
        private Health _health;
        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private Collider _collider;
        [SerializeField]
        private LayerMask _excludeLayersWhenDie;
        [SerializeField]
        private float _destroyDuration = 1f;

        private EntityIdentity _myIdentity;
        private void Awake()
        {
            _health.OnDeath += OnDeath;
            _myIdentity = GetComponent<EntityIdentity>();
            Initialize();
        }

        public async void OnDeath(EntityIdentity instigatorIdentity)
        {
            GameplayEvents.OnKilled?.Invoke(new ObjectiveProgressEventData(instigatorIdentity, _myIdentity));

            _rigidbody.excludeLayers = _excludeLayersWhenDie;
            _collider.excludeLayers = _excludeLayersWhenDie;
            OnDeath(_health);
            await UniTask.Delay(TimeSpan.FromSeconds(_destroyDuration));
            Destroy(_health.gameObject);

        }

        public abstract void Initialize();
        public abstract void OnDeath(Health health);
    }
}
