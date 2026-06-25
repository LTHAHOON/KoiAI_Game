using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : FollowableUI
{
    private Slider _healthBarSlider;
    private float _maxHealth = -1f;

    public void Init(float curHealth, float maxHealth)
    {
        _healthBarSlider = GetComponent<Slider>();
        _maxHealth = maxHealth;
        float normHealth = Mathf.Clamp01(curHealth / maxHealth);
        _healthBarSlider.value = normHealth;
    }
    
    public void ChangeHealthBar(float currentHealth)
    {
        if (_maxHealth <= 0)
        {
            return;
        }
        float normHealth = Mathf.Clamp01(currentHealth / _maxHealth);
        _healthBarSlider.value = normHealth;
    }

}
