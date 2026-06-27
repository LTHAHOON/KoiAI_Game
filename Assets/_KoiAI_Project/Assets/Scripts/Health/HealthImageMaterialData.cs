using UnityEngine;

namespace KoiAI.Health
{
    [CreateAssetMenu(fileName = "new HealthImageMaterialData", menuName = "Health/HealthBarData/HealthImageMaterialData")]
    public class HealthImageMaterialData : HealthBarData
    {
        [SerializeField]
        [ColorUsage(true, true)]
        private Color _healthColor;
    
        public Color HealthColor => _healthColor;
    }
}
