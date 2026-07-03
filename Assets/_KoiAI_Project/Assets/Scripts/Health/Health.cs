using R3;
using System;
using UnityEngine;

namespace KoiAI.Health
{
    using KoiAI.Interact;
    using KoiAI.ItemProp;
    using KoiAI.Utilities;

    public class Health : UIFollowHandle, IHealthProvider
    {
        [SerializeField]
        private HealthData _healthData;

        private ReactiveProperty<float> _currentHealth = new(0);
        private HealthBar _healthBar;
        private bool _isDelayChanging = false;
    
        private void Awake()
        {
            _currentHealth.Value = _healthData.MaxHealth;
            _currentHealth
                .Skip(1)
                .Subscribe(healthValue =>
                {
                    HealthBarManager.Instance.ChangeHealth(this);
                    if (healthValue <= 0)
                    {
                        OnDead();
                    }
                }).AddTo(this);
        }
    
        private void Start()
        {
            bool bGet = HealthBarManager.Instance.TryGetHealthBar(this, out HealthBar healthBar);
            if (bGet)
            {
                SetUITargetObject(healthBar);
                _healthBar = healthBar;
            }
            else
            {
                _healthBar = HealthBarManager.Instance.CreateHealthBar(this);
                SetUITargetObject(_healthBar);
            }
            _healthBar.Init(_healthData.HealthBarData, _currentHealth.CurrentValue, _healthData.MaxHealth);
        }

        public void ChangeHealth(float amount)
        {
            _currentHealth.Value += amount;
        }

        public void ChangeDelayHealth(float amount, float delayTime)
        {
            if (_isDelayChanging)
            {
                return;
            }
            _isDelayChanging = true;
            Observable.Timer(TimeSpan.FromSeconds(delayTime))
                .Subscribe(_ =>
                {
                    _isDelayChanging = false;
                    ChangeHealth(amount);
                });
        }
    
        private void OnDead()
        {
            Debug.Log("Dead");
        }
    
        public HealthBar GetHealthBar() => _healthBar;


        public void RefreshItemPickUpCondition(ItemPickUpCondition currentConditionData, ItemPickUpCondition compareCondition)
        {
            var conditionData = currentConditionData.hpCompareCondition;

            conditionData.SetCompareValue(CurrentHealthRatio);
            currentConditionData.hpCompareCondition = conditionData;
        }

        public float CurrentHealthRatio => Mathf.Clamp01(CurrentHealth / MaxHealth);
        public float CurrentHealth => _currentHealth.CurrentValue;
        public float MaxHealth => _healthData.MaxHealth;
        public HealthData HealthData => _healthData;    
    }
}
