using UnityEngine;

namespace KoiAI.Health
{
    [CreateAssetMenu(fileName = "HealthData", menuName = "Health/HealthData", order = 0)]
    public class HealthData : ScriptableObject
    {
        [SerializeField]
        private float _maxHealth;
        [SerializeField]
        private HealthBarData _healthBarData;
    
        public float MaxHealth => _maxHealth;
        public HealthBarData HealthBarData => _healthBarData;
    }
}
