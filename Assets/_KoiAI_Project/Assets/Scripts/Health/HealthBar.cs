using UnityEngine;

namespace KoiAI.Health
{
    using KoiAI.Utilities;
    
    public abstract class HealthBar : FollowableUI
    {
        private float _maxHealth = -1f;
        public void Init(HealthBarData healthBarData, float curHealth, float maxHealth)
        {
            _maxHealth = maxHealth;
            float normHealth = Mathf.Clamp01(curHealth / maxHealth);
            SetHealthBarValue(normHealth);
            Init(healthBarData);
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

        protected abstract void Init(HealthBarData healthBarData);
        protected abstract void SetHealthBarValue(float normHealth);
    }
}
