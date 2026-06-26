using UnityEngine;
using UnityEngine.UI;

public class HealthSliderBar : HealthBar
{
    [SerializeField]
    private Slider _healthSlider;

    public override void SetHealthBarValue(float normHealth)
    {
        _healthSlider.value = normHealth;
    }
}
