using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using KoiAI.Core;
using UnityEngine;

namespace KoiAI.Health
{
    [RequireComponent(typeof(Health), typeof(Collider), typeof(EntityIdentity))]
    public abstract class Death : MonoBehaviour
    {
        [SerializeField]
        private LayerMask _excludeLayersWhenDie;
        [SerializeField]
        private float _destroyDuration = 1f;
        [SerializeField]
        private float _ditherDuration = 0.6f;
        [SerializeField]
        private ParticleSystem _deathParticlePrefab;
        [SerializeField]
        private Vector3 _deathParticlePosOffset;

        private static readonly int DitherAlphaID = Shader.PropertyToID("_Dither_Alpha");
        private Material[] _materials;
        private EntityIdentity _myIdentity;
        private Rigidbody _rigidbody;
        private Health _health;
        private Collider _collider;

        private void Awake()
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null && meshRenderers.Length > 0)
            {
                _materials = meshRenderers.Select(meshRenderer => meshRenderer.material).ToArray();
            }
            else
            {
                 SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
                {
                    _materials = skinnedMeshRenderers.Select(skinnedMeshRenderer => skinnedMeshRenderer.material).ToArray();
                }
            }
            _health = GetComponent<Health>();
            _myIdentity = GetComponent<EntityIdentity>();
            _collider = GetComponent<Collider>();
            TryGetComponent<Rigidbody>(out _rigidbody);
            _health.OnDeath += OnDeath;
            Initialize();
        }

        private async UniTask OnDeath(EntityIdentity instigatorIdentity)
        {
            GameplayEvents.OnKilled?.Invoke(new ObjectiveProgressEventData(instigatorIdentity, _myIdentity));
            if (_rigidbody)
            {
                _rigidbody.excludeLayers = _excludeLayersWhenDie;
            }
            _collider.excludeLayers = _excludeLayersWhenDie;
            ParticleSystem deathParticle = null;
            if(_deathParticlePrefab)
            {
                deathParticle = Instantiate(_deathParticlePrefab);
                deathParticle.transform.position = transform.position + _deathParticlePosOffset;
                if(deathParticle)
                {
                    deathParticle.Play();    
                }    
            }
            
            OnDeath(_health);

            if (_materials != null)
            {
                for (int i = 0; i < _materials.Length; ++i)
                {
                    _materials[i].DOFloat(0f, DitherAlphaID, _ditherDuration);
                }
            }
            await UniTask.WhenAll(DeathDelayTask(), WaitUntilStopDeathParticle(deathParticle));
            Destroy(deathParticle.gameObject);
            Destroy(_health.gameObject);
        }

        private async UniTask DeathDelayTask()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_destroyDuration));
        }
        
        private async UniTask WaitUntilStopDeathParticle(ParticleSystem deathParticle)
        {
            if(deathParticle)
            {
                await UniTask.WaitUntil(() => deathParticle.isStopped);
            }
        }


        public abstract void Initialize();
        public abstract void OnDeath(Health health);
    }
}
