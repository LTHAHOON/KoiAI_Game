using UnityEngine;

namespace KoiAI.Item
{
    using KoiAI.Health;
    using KoiAI.Utilities;
    using KoiAI.Skin;

    public class CannonBallController : MonoBehaviour
    {
        private CannonBallSkin _cannonBallSkin;
        private HitEffectTrigger _hitEffectTrigger;

        private CannonBallData _cannonBallData;
        public bool Init(CannonBallData cannonBallData, LayerMask targetLayerMask)
        {
            _cannonBallData = cannonBallData;
            if (!_cannonBallSkin)
            {
                _cannonBallSkin = GetComponentInChildren<CannonBallSkin>();
            }
            if (!_hitEffectTrigger)
            {
                _hitEffectTrigger = GetComponentInChildren<HitEffectTrigger>();
            }
            
            _hitEffectTrigger.Init(cannonBallData.MaxOverlapCount,cannonBallData.RadiusExplosion, OnHit,targetLayerMask);
            return true;
        }

        public void SetSkin(CannonBallSkin cannonBallSkin)
        {
            _cannonBallSkin = cannonBallSkin;
        }

        private void OnHit(Collider[] targetColliders, int hitCount)
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

            for (int i = 0; i < targetColliders.Length; i++)
            {   
                if (targetColliders[i] && targetColliders[i].TryGetComponent(out Health health))
                {
                    health.ChangeHealth(-_cannonBallData.Damage);
                }
            }
        }

        public CannonBallSkin GetCannonBallSkin() => _cannonBallSkin;
    }
}
