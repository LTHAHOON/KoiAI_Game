using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.Health
{
    public class HealthImageMaterialBar : HealthBar
    {
        [SerializeField]
        private Image _healthImage;
        [SerializeField]
        private string _healthColorProperty = "_Main_Color";
        [SerializeField]
        private string _healthAmountProperty = "_Gauge_Amount";
        [SerializeField]
        private RectTransform _healthBarRect;
    
        private int _healthAmountID = -1;
        private int _healthColorID = -1;
        private Material _material;
    
        private void OnDisable()
        {
            if (_material && _material.HasProperty(_healthAmountID))
            {
                _material.SetFloat(_healthAmountID, 1);
            }
        }
    
        protected override void Init(HealthBarData healthBarData)
        {
            if (healthBarData is not HealthImageMaterialData healthImageMaterialData || !_healthBarRect || !_healthImage)
            {
                return;
            }

            if(!_material)
            {
                InitMaterialProperty();
                if (!_material || !_material.HasProperty(_healthColorProperty)|| !_material.HasProperty(_healthAmountID))
                {
                    return;
                }
            }
            _material.SetFloat(_healthAmountID, 1);
            _material.SetColor(_healthColorID, healthImageMaterialData.HealthColor);
            _healthBarRect.rect.Set(0f, 0f, healthImageMaterialData.Width, healthImageMaterialData.Height);
        }
    
        private void InitMaterialProperty()
        {
            _material = _healthImage.material;
            _healthColorID = Shader.PropertyToID(_healthColorProperty);
            _healthAmountID = Shader.PropertyToID(_healthAmountProperty);
        }
    
        protected override void SetHealthBarValue(float normHealth)
        {
            if (!_material || !_material.HasProperty(_healthColorProperty))
            {
                return;
            }

            //같은 material이 있을 경우 주의 요망
            _material.SetFloat(_healthAmountID, normHealth);
        }

    }
}
