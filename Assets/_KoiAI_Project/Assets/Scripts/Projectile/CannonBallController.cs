using KoiAI.Weapon;
using UnityEngine;

namespace KoiAI.Projectile
{
    public class CannonBallController : MonoBehaviour
    {
        [SerializeField]
        private CannonBallSkin _cannonBallSkin;
        [SerializeField] 
        private ExplosionTrigger _explosionTrigger;
        private CannonBallData _cannonBallData;
        public void Init(CannonBallData cannonBallData, LayerMask targetLayerMask)
        {
            _cannonBallData = cannonBallData;
            _explosionTrigger.Init(cannonBallData.MaxOverlapCount,cannonBallData.RadiusExplosion, OnExplosion,targetLayerMask);
        }

        private void OnExplosion(Collider[] targetColliders, int hitCount)
        {
            if (!_cannonBallData)
            {
                return;
            }
            _cannonBallSkin.TrailRenderer.enabled = false;
            if (hitCount <= 0)
            {
                return;
            }
        
            for (int i = 0; i < hitCount; i++)
            {
                if (!targetColliders[i])
                {
                    continue;
                }
                if (targetColliders[i].TryGetComponent(out Health.Health health))
                {
                    health.ChangeHealth(-_cannonBallData.Damage);
                }
            }
        }

        public CannonBallSkin GetCannonBallSkin() => _cannonBallSkin;
    }
}
