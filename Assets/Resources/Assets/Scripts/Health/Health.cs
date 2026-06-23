using System;
using R3;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float _maxHealth = 100;

    private ReactiveProperty<float> _currentHealth = new(0);
    private HealthBar _healthBar;
    private bool _isDelayChanging = false;
    private void Awake()
    {
        _currentHealth.Value = _maxHealth;
        _currentHealth.Subscribe(healthValue =>
        {
            HealthBarManager.Instance.ChangeHealth(this);
            if (healthValue <= 0)
            {
                OnDead();
            }
        });
    }
    
    private void Start()
    {
        _healthBar = HealthBarManager.Instance.CreateHealthBar();
        _healthBar.Init(_currentHealth.CurrentValue, _maxHealth);
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
    public float CurrentHealth => _currentHealth.CurrentValue;
    public float MaxHealth => _maxHealth;
}
