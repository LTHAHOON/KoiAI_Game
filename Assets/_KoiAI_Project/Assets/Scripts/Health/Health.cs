using System;
using KoiAI.UI;
using R3;
using UnityEngine;

namespace KoiAI.Health
{
    public class Health : UIFollowHandle
    {
        [SerializeField]
        private HealthData _healthData;

        private ReactiveProperty<float> _currentHealth = new(0);
        private ReactiveProperty<HealthBar> _healthBar = new();
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

            _healthBar
                .Where(healthBar => healthBar != null)
                .Subscribe(healthBar =>
                {
                    if(healthBar.IsBlockFollowUI)
                    {
                        return;
                    }
                    var connector = GetUIFollowConnector();
                    connector.OnNext(healthBar);
                }).AddTo(this);
        }
    
        private void Start()
        {
            bool bGet = HealthBarManager.Instance.TryGetHealthBar(this, out HealthBar healthBar);
            if (bGet)
            {
                _healthBar.Value = healthBar;
            }
            else
            {
                _healthBar.Value = HealthBarManager.Instance.CreateHealthBar(this);
            }
            _healthBar.Value.Init(_healthData.HealthBarData, _currentHealth.CurrentValue, _healthData.MaxHealth);
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
    
        public HealthBar GetHealthBar() => _healthBar.Value;
        public float CurrentHealth => _currentHealth.CurrentValue;
        public float MaxHealth => _healthData.MaxHealth;
        public HealthData HealthData => _healthData;    
    }
}
