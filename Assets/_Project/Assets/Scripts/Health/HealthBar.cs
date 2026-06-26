using UnityEngine;
using UnityEngine.UI;


public abstract class HealthBar : FollowableUI
{
    private float _maxHealth = -1f;

    public void Init(float curHealth, float maxHealth)
    {
        _maxHealth = maxHealth;
        float normHealth = Mathf.Clamp01(curHealth / maxHealth);
        SetHealthBarValue(normHealth);
    }


    public void ChangeHealthBar(float currentHealth)
    {
        if (_maxHealth <= 0)
        {
            return;
        }
        float normHealth = Mathf.Clamp01(currentHealth / _maxHealth);
        SetHealthBarValue(normHealth);
    }

    public abstract void SetHealthBarValue(float normHealth);
}
