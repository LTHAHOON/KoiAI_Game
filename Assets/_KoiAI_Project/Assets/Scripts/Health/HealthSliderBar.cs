using UnityEngine;
using UnityEngine.UI;

namespace KoiAI.Health
{
    public class HealthSliderBar : HealthBar
    {
        [SerializeField]
        private Slider _healthSlider;
        [SerializeField]
        private Image _healthImage;
        [SerializeField]
        private Image _healthBackgroundImage;
        [SerializeField]
        private RectTransform _healthBarRect;
    
        protected override void Init(HealthBarData healthBarData)
        {
            if (healthBarData is not HealthSliderData healthSliderData 
                || !_healthBarRect || !_healthSlider || !_healthImage || !_healthBackgroundImage)
            {
                return;
            }
            _healthBarRect.rect.Set(0f, 0f, healthSliderData.Width, healthSliderData.Height);
            _healthImage.color = healthSliderData.HealthColor;
            _healthBackgroundImage.color = healthSliderData.HealthBackgroundColor;
        }

        protected override void SetHealthBarValue(float normHealth)
        {
            _healthSlider.value = normHealth;
        }
    }
}
