using UnityEngine;

namespace KoiAI.Item
{
    using KoiAI.Health;
    using KoiAI.Utilities;
    using KoiAI.Skin;

    public class CannonBallController : MonoBehaviour
    {
        [SerializeField]
        private CannonBallSkin _cannonBallSkin;
        [SerializeField] 
        private HitEffectTrigger _hitEffectTrigger;

        private CannonBallData _cannonBallData;
        public void Init(CannonBallData cannonBallData, LayerMask targetLayerMask)
        {
            _cannonBallData = cannonBallData;
            _hitEffectTrigger.Init(cannonBallData.MaxOverlapCount,cannonBallData.RadiusExplosion, OnHit,targetLayerMask);
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
                if (targetColliders[i].TryGetComponent(out Health health))
                {
                    health.ChangeHealth(-_cannonBallData.Damage);
                }
            }
        }

        public CannonBallSkin GetCannonBallSkin() => _cannonBallSkin;
    }
}
