using UnityEngine;

namespace KoiAI.Health
{
    [CreateAssetMenu(fileName = "new HealthSliderData", menuName = "Health/HealthBarData/HealthSliderData")]
    public class HealthSliderData : HealthBarData
    {
        [SerializeField]
        private Color _healthColor;
        [SerializeField]
        private Color _healthBackgroundColor;
    
        public Color HealthColor => _healthColor;
        public Color HealthBackgroundColor => _healthBackgroundColor;
    }
}
