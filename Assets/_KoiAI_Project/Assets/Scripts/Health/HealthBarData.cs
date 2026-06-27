using UnityEngine;

namespace KoiAI.Health
{
    public enum HealthBarType
    {
        BaseUI,
        FollowUI,
    }

    public abstract class HealthBarData : ScriptableObject
    {
        [SerializeField] 
        private HealthBarType _healthBarType;
        [SerializeField]
        private HealthBar _healthBarPrefab;
        [SerializeField]
        private float _width;
        [SerializeField]
        private float _height;
    
        public HealthBarType HealthBarType => _healthBarType;
        public HealthBar HealthBarPrefab => _healthBarPrefab;
        public float Width => _width;
        public float Height => _height;
    }
}