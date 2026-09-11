using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KoiAI.Health
{
    [RequireComponent(typeof(Health))]
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

        private void Awake()
        {
            _health.OnDeath += OnDeath;
            Initialize();
        }

        public async void OnDeath()
        {
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
