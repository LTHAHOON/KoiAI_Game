using UnityEngine;

namespace KoiAI.Skin
{
    public class CannonBallSkin : ProjectileSkin
    {
        [SerializeField]
        private Rigidbody _rigid;
        [SerializeField]
        private TrailRenderer _trailRenderer;

        private void Awake()
        {
            if (!_rigid)
            {
                _rigid = GetComponent<Rigidbody>();
            }
            if (!_trailRenderer)
            {
                _trailRenderer = GetComponent<TrailRenderer>();
            }
        }

        public Rigidbody Rigidbody => _rigid;
        public TrailRenderer TrailRenderer => _trailRenderer;
    }
}
